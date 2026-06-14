using System.Collections.Immutable;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding;

partial class Binder
{
    private BoundTableReference BindTableReference(TableReferenceSyntax node)
    {
        return Bind(node, BindTableReferenceInternal);
    }

    private BoundTableReference BindTableReferenceInternal(TableReferenceSyntax node)
    {
        switch (node.Kind)
        {
            case SyntaxKind.ParenthesizedTableReference:
                return BindParenthesizedTableReference((ParenthesizedTableReferenceSyntax)node);

            case SyntaxKind.NamedTableReference:
                return BindNamedTableReference((NamedTableReferenceSyntax)node);

            case SyntaxKind.CrossJoinedTableReference:
                return BindCrossJoinedTableReference((CrossJoinedTableReferenceSyntax)node);

            case SyntaxKind.CrossAppliedTableReference:
                return BindCrossAppliedTableReference((CrossAppliedTableReferenceSyntax)node);

            case SyntaxKind.OuterAppliedTableReference:
                return BindOuterAppliedTableReference((OuterAppliedTableReferenceSyntax)node);

            case SyntaxKind.InnerJoinedTableReference:
                return BindInnerJoinedTableReference((InnerJoinedTableReferenceSyntax)node);

            case SyntaxKind.OuterJoinedTableReference:
                return BindOuterJoinedTableReference((OuterJoinedTableReferenceSyntax)node);

            case SyntaxKind.DerivedTableReference:
                return BindDerivedTableReference((DerivedTableReferenceSyntax)node);

            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    private BoundTableReference BindParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
    {
        return BindTableReference(node.TableReference);
    }

    private BoundTableReference BindNamedTableReference(NamedTableReferenceSyntax node)
    {
        var symbols = LookupTable(node.TableName).ToImmutableArray();

        if (symbols.Length == 0)
        {
            Diagnostics.ReportUndeclaredTable(node);

            var errorTable = new ErrorTableSymbol(node.TableName.ValueText);
            var errorAlias = node.Alias is null
                               ? errorTable.Name
                               : node.Alias.Identifier.ValueText;
            var errorInstance = new TableInstanceSymbol(errorAlias, errorTable);
            return new BoundNamedTableReference(errorInstance);
        }

        if (symbols.Length > 1)
            Diagnostics.ReportAmbiguousTable(node.TableName, symbols);

        var table = symbols[0];
        var aliasIdentifier = node.Alias is not null
            ? node.Alias.Identifier
            : node.TableName;

        var alias = aliasIdentifier.ValueText;

        var tableInstance = new TableInstanceSymbol(alias, table);

        QueryState.IntroducedTables.Add(tableInstance, aliasIdentifier);

        return new BoundNamedTableReference(tableInstance);
    }

    private BoundTableReference BindCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
    {
        var left = BindTableReference(node.Left);
        var right = BindTableReference(node.Right);
        return new BoundJoinTableReference(BoundJoinType.Inner, left, right, null);
    }

    private BoundTableReference BindCrossAppliedTableReference(CrossAppliedTableReferenceSyntax node)
    {
        var left = BindTableReference(node.Left);

        // Unlike a join, the right side of an APPLY may reference the left's columns, so we
        // bind it with the left's tables in scope. A correlated reference then resolves to the
        // left's column instances -- the same mechanism a correlated subquery uses -- which is
        // what makes the algebrizer's dependent join correlate to the left.
        var leftBinder = CreateLocalBinder(left.GetDeclaredTableInstances());
        var right = leftBinder.BindTableReference(node.Right);

        return new BoundApplyTableReference(BoundJoinType.Inner, left, right);
    }

    private BoundTableReference BindOuterAppliedTableReference(OuterAppliedTableReferenceSyntax node)
    {
        var left = BindTableReference(node.Left);

        // Same correlated binding as CROSS APPLY: the right is bound with the left's tables in
        // scope so it may reference the left's columns. The only difference is the apply kind --
        // OUTER APPLY is a left-outer apply, so a left row with no matching right rows survives
        // (null-padded) instead of being dropped, which the algebrizer maps to a left-outer
        // dependent join.
        var leftBinder = CreateLocalBinder(left.GetDeclaredTableInstances());
        var right = leftBinder.BindTableReference(node.Right);

        return new BoundApplyTableReference(BoundJoinType.LeftOuter, left, right);
    }

    private BoundTableReference BindInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
    {
        var left = BindTableReference(node.Left);
        var right = BindTableReference(node.Right);

        var binder = CreateJoinConditionBinder(left, right);
        var condition = binder.BindExpression(node.Condition);

        if (condition.Type.IsNonBoolean())
            Diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

        return new BoundJoinTableReference(BoundJoinType.Inner, left, right, condition);
    }

    private BoundTableReference BindOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
    {
        var joinType = node.TypeKeyword.Kind == SyntaxKind.LeftKeyword
                           ? BoundJoinType.LeftOuter
                           : node.TypeKeyword.Kind == SyntaxKind.RightKeyword
                                 ? BoundJoinType.RightOuter
                                 : BoundJoinType.FullOuter;

        var left = BindTableReference(node.Left);
        var right = BindTableReference(node.Right);

        var binder = CreateJoinConditionBinder(left, right);
        var condition = binder.BindExpression(node.Condition);

        if (condition.Type.IsNonBoolean())
            Diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

        return new BoundJoinTableReference(joinType, left, right, condition);
    }

    private BoundTableReference BindDerivedTableReference(DerivedTableReferenceSyntax node)
    {
        // TODO: Ensure query has no ORDER BY unless TOP is also specified

        var query = BindQuery(node.Query);

        var namedQueryColumns = query.OutputColumns.Where(c => !string.IsNullOrEmpty(c.Name));
        var columns = new List<ColumnSymbol>();
        var valueFromColumn = new Dictionary<ColumnSymbol, IBoundValue>();

        foreach (var queryColumn in namedQueryColumns)
        {
            var columnSymbol = new ColumnSymbol(queryColumn.Name, queryColumn.Type);
            columns.Add(columnSymbol);
            valueFromColumn.Add(columnSymbol, queryColumn.BoundValue);
        }

        var derivedTable = new DerivedTableSymbol(columns);
        var aliasFactory = new Func<TableInstanceSymbol, ColumnSymbol, IBoundValue>((_, c) => valueFromColumn[c]);
        var derivedTableInstance = new TableInstanceSymbol(node.Name.ValueText, derivedTable, aliasFactory);
        var boundTableReference = new BoundDerivedTableReference(derivedTableInstance, query);

        QueryState.IntroducedTables.Add(derivedTableInstance, node.Name);

        return boundTableReference;
    }
}
