using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Formatting;

// Decides the gaps that structure demands -- line breaks, indentation, and the tabular pad -- by
// walking the tree. Everything it doesn't have an opinion about is left to SpacingRules, which is
// why this only visits the constructs whose layout is interesting and lets ordinary expressions
// fall through.
//
// It annotates tokens rather than emitting them. A walker that produced the token sequence itself
// would drop text the day a node type is missed here; keyed by token, a missed node just means its
// gaps keep the default spacing.
internal sealed class LayoutWalker
{
    private readonly FormattingOptions _options;
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private readonly Dictionary<SyntaxToken, int> _indexOf;
    private readonly Dictionary<SyntaxToken, Gap> _gaps = [];
    private readonly List<GapGroup> _groups = [];

    private int _group = -1;

    private LayoutWalker(FormattingOptions options, ImmutableArray<SyntaxToken> tokens, Dictionary<SyntaxToken, int> indexOf)
    {
        _options = options;
        _tokens = tokens;
        _indexOf = indexOf;
    }

    public static (Dictionary<SyntaxToken, Gap> Gaps, ImmutableArray<GapGroup> Groups) Compute(
        CompilationUnitSyntax root,
        FormattingOptions options,
        ImmutableArray<SyntaxToken> tokens,
        Dictionary<SyntaxToken, int> indexOf)
    {
        var walker = new LayoutWalker(options, tokens, indexOf);
        walker.VisitRoot(root);
        return (walker._gaps, walker._groups.ToImmutableArray());
    }

    private void VisitRoot(CompilationUnitSyntax root)
    {
        switch (root.Root)
        {
            case QuerySyntax query:
                VisitQuery(query, 0);
                break;
            case ExpressionSyntax expression:
                VisitExpression(expression, 0);
                break;
        }
    }

    // -- Queries ------------------------------------------------------------------------------

    private void VisitQuery(QuerySyntax node, int indent)
    {
        switch (node)
        {
            case SelectQuerySyntax select:
                VisitSelectQuery(select, indent);
                break;
            case OrderedQuerySyntax ordered:
                VisitOrderedQuery(ordered, indent);
                break;
            case UnionQuerySyntax union:
                VisitBinaryQuery(union.LeftQuery, union.UnionKeyword, union.RightQuery, indent);
                break;
            case IntersectQuerySyntax intersect:
                VisitBinaryQuery(intersect.LeftQuery, intersect.IntersectKeyword, intersect.RightQuery, indent);
                break;
            case ExceptQuerySyntax except:
                VisitBinaryQuery(except.LeftQuery, except.ExceptKeyword, except.RightQuery, indent);
                break;
            case ParenthesizedQuerySyntax parenthesized:
                VisitParenthesizedQuery(parenthesized, indent);
                break;
            case CommonTableExpressionQuerySyntax cte:
                VisitCommonTableExpressionQuery(cte, indent);
                break;
        }
    }

    private void VisitSelectQuery(SelectQuerySyntax node, int indent)
    {
        var payload = GetPayload(indent);

        VisitSelectClause(node.SelectClause, payload);

        if (node.FromClause is not null)
            VisitFromClause(node.FromClause, indent, payload);

        if (node.WhereClause is not null)
            VisitPredicateClause(node.WhereClause.WhereKeyword, node.WhereClause.Predicate, indent, payload);

        if (node.GroupByClause is not null)
            VisitGroupByClause(node.GroupByClause, indent, payload);

        if (node.HavingClause is not null)
            VisitPredicateClause(node.HavingClause.HavingKeyword, node.HavingClause.Predicate, indent, payload);
    }

    private void VisitSelectClause(SelectClauseSyntax node, int payload)
    {
        var restore = PushGroup(node);

        // The gap before SELECT belongs to whatever introduced the query -- a CTE body, a union, a
        // subquery paren -- so the clause never sets its own.

        // DISTINCT and TOP are modifiers of the keyword, not payload, so they stay on its line and
        // the column list starts after them.
        var modifier = node.DistinctAllKeyword ?? node.TopClause?.TopKeyword;
        if (modifier is not null)
            SetGap(modifier, GetKeywordGapKind(), payload);

        // Tabular keeps the first column on the keyword's line, which is the whole point of the pad.
        // Stacked puts the list below the keyword, so the first column breaks like the rest.
        var firstColumnGap = _options.Layout == LayoutStyle.Tabular
                                ? modifier is null ? GapKind.Pad : GapKind.Space
                                : GetSelectColumnBreak(node.Columns.Count);

        for (var i = 0; i < node.Columns.Count; i++)
        {
            var column = node.Columns[i];
            var first = column.FirstToken(includeZeroLength: true);

            SetGap(first, i == 0 ? firstColumnGap : GetSelectColumnBreak(node.Columns.Count), payload);

            VisitSelectColumn(column, payload);
        }

        _group = restore;
    }

    private void VisitSelectColumn(SelectColumnSyntax node, int lineIndent)
    {
        if (node is ExpressionSelectColumnSyntax column)
            VisitExpression(column.Expression, lineIndent);
    }

    private void VisitFromClause(FromClauseSyntax node, int indent, int payload)
    {
        var restore = PushGroup(node);

        SetGap(node.FromKeyword, GapKind.Line, indent);

        // Where a join line starts. Tabular hangs its payload off KeywordColumn, so "the level the
        // FROM payload is on" is that column; stacked keeps the payload on the keyword's line, so
        // it's the keyword's own column.
        var fromLevel = _options.Layout == LayoutStyle.Tabular ? payload : indent;

        for (var i = 0; i < node.TableReferences.Count; i++)
        {
            var tableReference = node.TableReferences[i];
            var first = tableReference.FirstToken(includeZeroLength: true);

            if (i == 0)
                SetGap(first, GetKeywordGapKind(), payload);
            else
                SetGap(first, GapKind.SoftLine, payload);

            VisitTableReference(tableReference, fromLevel);
        }

        _group = restore;
    }

    private void VisitPredicateClause(SyntaxToken keyword, ExpressionSyntax predicate, int indent, int payload)
    {
        SetGap(keyword, GapKind.Line, indent);
        SetGap(predicate.FirstToken(includeZeroLength: true), GetKeywordGapKind(), payload);
        VisitExpression(predicate, payload);
    }

    private void VisitGroupByClause(GroupByClauseSyntax node, int indent, int payload)
    {
        var restore = PushGroup(node);

        SetGap(node.GroupKeyword, GapKind.Line, indent);

        // The pad falls between GROUP and BY rather than after them, which is what lands BY in the
        // payload column and keeps the two-word keywords aligned with the one-word ones.
        SetGap(node.ByKeyword, GetKeywordGapKind(), payload);

        VisitSoftList(node.Columns, payload, (column, lineIndent) => VisitExpression(column.Expression, lineIndent));

        _group = restore;
    }

    private void VisitOrderedQuery(OrderedQuerySyntax node, int indent)
    {
        VisitQuery(node.Query, indent);

        var payload = GetPayload(indent);
        var last = node.LastToken(includeZeroLength: true);
        var restore = PushGroup(node.OrderKeyword, last);

        SetGap(node.OrderKeyword, GapKind.Line, indent);
        SetGap(node.ByKeyword, GetKeywordGapKind(), payload);

        VisitSoftList(node.Columns, payload, (column, lineIndent) => VisitExpression(column.ColumnSelector, lineIndent));

        _group = restore;
    }

    private void VisitBinaryQuery(QuerySyntax left, SyntaxToken keyword, QuerySyntax right, int indent)
    {
        VisitQuery(left, indent);
        SetGap(keyword, GapKind.Line, indent);
        SetGap(right.FirstToken(includeZeroLength: true), GapKind.Line, indent);
        VisitQuery(right, indent);
    }

    private void VisitParenthesizedQuery(ParenthesizedQuerySyntax node, int indent)
    {
        VisitParenthesizedQuery(node, node.Query, node.RightParenthesisToken, indent);
    }

    private void VisitParenthesizedQuery(SyntaxNode node, QuerySyntax query, SyntaxToken rightParenthesis, int lineIndent)
    {
        var restore = PushGroup(node);
        var inner = lineIndent + _options.IndentSize;

        SetGap(query.FirstToken(includeZeroLength: true), GapKind.SoftLine, inner);
        VisitQuery(query, inner);
        SetGap(rightParenthesis, GapKind.SoftLine, lineIndent);

        _group = restore;
    }

    private void VisitCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node, int indent)
    {
        var inner = indent + _options.IndentSize;

        foreach (var commonTableExpression in node.CommonTableExpressions)
        {
            // The body always breaks: a CTE that fits on one line is rare enough that treating it
            // as the exception costs more than it saves.
            SetGap(commonTableExpression.Query.FirstToken(includeZeroLength: true), GapKind.Line, inner);
            VisitQuery(commonTableExpression.Query, inner);
            SetGap(commonTableExpression.RightParenthesisToken, GapKind.Line, indent);
        }

        SetGap(node.Query.FirstToken(includeZeroLength: true), GapKind.Line, indent);
        VisitQuery(node.Query, indent);
    }

    // -- Table references ---------------------------------------------------------------------

    private void VisitTableReference(TableReferenceSyntax node, int fromLevel)
    {
        switch (node)
        {
            case JoinedTableReferenceSyntax join:
                VisitJoinedTableReference(join, fromLevel);
                break;
            case DerivedTableReferenceSyntax derived:
                VisitParenthesizedQuery(derived, derived.Query, derived.RightParenthesisToken, fromLevel);
                break;
            case ParenthesizedTableReferenceSyntax parenthesized:
                VisitTableReference(parenthesized.TableReference, fromLevel);
                break;
        }
    }

    private void VisitJoinedTableReference(JoinedTableReferenceSyntax node, int fromLevel)
    {
        // Left-deep chains are flattened: every join in a chain gets the same column, rather than
        // one more level of indentation per join.
        VisitTableReference(node.Left, fromLevel);

        var joinColumn = _options.Joins == JoinIndentation.Indented
                            ? fromLevel + _options.IndentSize
                            : fromLevel;

        // The join's own keywords start at whatever follows the left side, which spares this from
        // enumerating which of INNER/LEFT/OUTER/CROSS/APPLY a given join spells out.
        SetGap(GetTokenAfter(node.Left), GapKind.Line, joinColumn);

        VisitTableReference(node.Right, joinColumn);

        if (node is ConditionedJoinedTableReferenceSyntax conditioned)
            VisitJoinCondition(conditioned, joinColumn);
    }

    private void VisitJoinCondition(ConditionedJoinedTableReferenceSyntax node, int joinColumn)
    {
        var onOwnLine = _options.On switch
        {
            OnPlacement.OwnLine => true,
            OnPlacement.OwnLineWhenMultiple => IsLogicalChain(node.Condition),
            _ => false
        };

        var conditionIndent = joinColumn + _options.IndentSize;

        if (onOwnLine)
            SetGap(node.OnKeyword, GapKind.Line, conditionIndent);

        // The condition indents under the join whether or not ON starts a line: a wrapped AND that
        // lands in the FROM column reads as another join.
        VisitExpression(node.Condition, conditionIndent);
    }

    // -- Expressions --------------------------------------------------------------------------

    // lineIndent is the column the logical line this expression sits on starts at, not the column
    // the expression itself starts at: indentation has to be decidable before anything is rendered,
    // and only line starts are known that early.
    private void VisitExpression(SyntaxNode node, int lineIndent)
    {
        switch (node)
        {
            case BinaryExpressionSyntax binary when IsLogicalOperator(binary.BinaryOperatorToken):
                VisitLogicalChain(binary, lineIndent);
                break;
            case CaseExpressionSyntax caseExpression:
                VisitCaseExpression(caseExpression, lineIndent);
                break;
            case ExistsSubselectSyntax exists:
                VisitParenthesizedQuery(exists, exists.Query, exists.RightParenthesisToken, lineIndent);
                break;
            case SingleRowSubselectSyntax singleRow:
                VisitParenthesizedQuery(singleRow, singleRow.Query, singleRow.RightParenthesisToken, lineIndent);
                break;
            case AllAnySubselectSyntax allAny:
                VisitExpression(allAny.Left, lineIndent);
                VisitParenthesizedQuery(allAny, allAny.Query, allAny.RightParenthesisToken, lineIndent);
                break;
            case InQueryExpressionSyntax inQuery:
                VisitExpression(inQuery.Expression, lineIndent);
                VisitParenthesizedQuery(inQuery, inQuery.Query, inQuery.RightParenthesisToken, lineIndent);
                break;
            case ArgumentListSyntax argumentList:
                VisitArgumentList(argumentList, lineIndent);
                break;
            default:
                foreach (var child in node.ChildNodes())
                {
                    if (child is QuerySyntax query)
                        VisitQuery(query, lineIndent + _options.IndentSize);
                    else
                        VisitExpression(child, lineIndent);
                }
                break;
        }
    }

    private void VisitLogicalChain(BinaryExpressionSyntax node, int lineIndent)
    {
        var restore = PushGroup(node);
        VisitLogicalChainPart(node, lineIndent);
        _group = restore;
    }

    // A chain breaks as a unit and the operator leads its line, which is the one layout convention
    // SQL shares across every style.
    private void VisitLogicalChainPart(ExpressionSyntax node, int lineIndent)
    {
        if (node is BinaryExpressionSyntax binary && IsLogicalOperator(binary.BinaryOperatorToken))
        {
            VisitLogicalChainPart(binary.Left, lineIndent);
            SetGap(binary.BinaryOperatorToken, GapKind.SoftLine, lineIndent);
            VisitExpression(binary.Right, lineIndent);
        }
        else
        {
            VisitExpression(node, lineIndent);
        }
    }

    private void VisitCaseExpression(CaseExpressionSyntax node, int lineIndent)
    {
        var restore = PushGroup(node);
        var inner = lineIndent + _options.IndentSize;

        if (node.InputExpression is not null)
            VisitExpression(node.InputExpression, lineIndent);

        foreach (var label in node.CaseLabels)
        {
            SetGap(label.WhenKeyword, GapKind.SoftLine, inner);
            VisitExpression(label.WhenExpression, inner);
            VisitExpression(label.ThenExpression, inner);
        }

        if (node.ElseLabel is not null)
        {
            SetGap(node.ElseLabel.ElseKeyword, GapKind.SoftLine, inner);
            VisitExpression(node.ElseLabel.Expression, inner);
        }

        SetGap(node.EndKeyword, GapKind.SoftLine, lineIndent);

        _group = restore;
    }

    private void VisitArgumentList(ArgumentListSyntax node, int lineIndent)
    {
        var restore = PushGroup(node);
        var inner = lineIndent + _options.IndentSize;

        foreach (var argument in node.Arguments)
        {
            SetGap(argument.FirstToken(includeZeroLength: true), GapKind.SoftLine, inner);
            VisitExpression(argument, inner);
        }

        SetGap(node.RightParenthesisToken, GapKind.SoftLine, lineIndent);

        _group = restore;
    }

    // -- Helpers ------------------------------------------------------------------------------

    private void VisitSoftList<TNode>(SeparatedSyntaxList<TNode> items, int payload, Action<TNode, int> visit)
        where TNode : SyntaxNode
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
                SetGap(items[i].FirstToken(includeZeroLength: true), GapKind.SoftLine, payload);

            visit(items[i], payload);
        }
    }

    private int GetPayload(int indent)
    {
        return _options.Layout == LayoutStyle.Tabular
                ? indent + _options.KeywordColumn
                : indent + _options.IndentSize;
    }

    // What separates a clause keyword from its payload.
    private GapKind GetKeywordGapKind()
    {
        return _options.Layout == LayoutStyle.Tabular ? GapKind.Pad : GapKind.Space;
    }

    // One per line is about a list; a projection of one column is not a list, and breaking it just
    // leaves the keyword stranded on a line of its own.
    private GapKind GetSelectColumnBreak(int columnCount)
    {
        return _options.SelectColumns == ListStyle.OnePerLine && columnCount > 1
                ? GapKind.Line
                : GapKind.SoftLine;
    }

    private static bool IsLogicalOperator(SyntaxToken token)
    {
        return token.Kind == SyntaxKind.AndKeyword || token.Kind == SyntaxKind.OrKeyword;
    }

    private static bool IsLogicalChain(ExpressionSyntax expression)
    {
        return expression is BinaryExpressionSyntax binary && IsLogicalOperator(binary.BinaryOperatorToken);
    }

    private SyntaxToken? GetTokenAfter(SyntaxNode node)
    {
        var last = node.LastToken(includeZeroLength: true);
        if (last is null || !_indexOf.TryGetValue(last, out var index) || index + 1 >= _tokens.Length)
            return null;

        return _tokens[index + 1];
    }

    private void SetGap(SyntaxToken? token, GapKind kind, int column)
    {
        if (token is null)
            return;

        _gaps[token] = new Gap(kind, column, _group);
    }

    private int PushGroup(SyntaxNode node)
    {
        return PushGroup(node.FirstToken(includeZeroLength: true), node.LastToken(includeZeroLength: true));
    }

    private int PushGroup(SyntaxToken? first, SyntaxToken? last)
    {
        var restore = _group;

        if (first is null || last is null)
            return restore;

        if (!_indexOf.TryGetValue(first, out var start) || !_indexOf.TryGetValue(last, out var end))
            return restore;

        _groups.Add(new GapGroup(start, end + 1, _group));
        _group = _groups.Count - 1;

        return restore;
    }
}
