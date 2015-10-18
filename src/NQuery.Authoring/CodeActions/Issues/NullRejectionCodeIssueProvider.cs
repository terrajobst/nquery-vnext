using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    // TODO: The following query results in duplicated warnings and fixes.
    //
    //       SELECT  e.FirstName,
    //               t.TerritoryDescription
    //       FROM    Employees e
    //                   RIGHT JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
    //                   RIGHT JOIN Territories t ON t.TerritoryID = et.TerritoryID
    //       WHERE   e.FirstName <> 'Immo'
    //
    //       The proper fix is to chnange the provider to run per query, rather
    //       than per outer join.
    //
    // TODO: The fix for changing the joins should affect all joins.
    //
    //       For example, in the following query:
    //
    //       SELECT  e.FirstName,
    //               t.TerritoryDescription
    //       FROM    Employees e
    //                   LEFT JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
    //                   LEFT JOIN Territories t ON t.TerritoryID = et.TerritoryID
    //       WHERE   t.TerritoryDescription.Length > 5
    //
    //       both LEFT JOIN clauses should be converted to an INNER JOIN.
    //
    // TODO: We need to handle null rejection by other join conditions.
    //
    //       For example, in the following the INNER JOIN rejects nulls from the
    //       LEFT join. In that case, the fix is rearranging the joins.
    //       Additional IS NULL checks will not work.
    //
    //       SELECT  e.FirstName,
    //               t.TerritoryDescription
    //       FROM    Employees e
    //                   LEFT JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
    //                   INNER JOIN Territories t ON t.TerritoryID = et.TerritoryID
    //
    internal sealed class NullRejectionCodeIssueProvider : CodeIssueProvider<SelectQuerySyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, SelectQuerySyntax node)
        {
            if (node.FromClause == null)
                return Enumerable.Empty<CodeIssue>();

            var whereClause = node.WhereClause?.Predicate;
            var havingClause = node.HavingClause?.Predicate;
            var outerJoins = node.FromClause.DescendantNodes()
                                            .OfType<OuterJoinedTableReferenceSyntax>()
                                            .Where(n => n.Ancestors().OfType<SelectQuerySyntax>().First() == node)
                                            .ToImmutableArray();

            if (!outerJoins.Any())
                return Enumerable.Empty<CodeIssue>();

            var predicates = new List<ExpressionSyntax>();
            SplitConjunctions(predicates, whereClause);
            SplitConjunctions(predicates, havingClause);

            var rejectedJoins = new List<RejectedOuterJoin>();

            foreach (var outerJoin in outerJoins)
            {
                var joinKind = outerJoin.TypeKeyword.Kind;
                var checkLeftForNulls = joinKind == SyntaxKind.FullKeyword || joinKind == SyntaxKind.RightKeyword;
                var checkRightForNull = joinKind == SyntaxKind.FullKeyword || joinKind == SyntaxKind.LeftKeyword;

                var leftTables = checkLeftForNulls ? semanticModel.GetDeclaredSymbols(outerJoin.Left).ToImmutableArray() : ImmutableArray<TableInstanceSymbol>.Empty;
                var rightTables = checkRightForNull ? semanticModel.GetDeclaredSymbols(outerJoin.Right).ToImmutableArray() : ImmutableArray<TableInstanceSymbol>.Empty;

                foreach (var predicate in predicates)
                {
                    var leftRejectedTables = leftTables.Where(t => semanticModel.IsRejectingNulls(predicate, t)).ToImmutableArray();
                    var rightRejectedTables = rightTables.Where(t => semanticModel.IsRejectingNulls(predicate, t)).ToImmutableArray();

                    if (leftRejectedTables.Any() || rightRejectedTables.Any())
                    {
                        var rejectedJoin = new RejectedOuterJoin(predicate, outerJoin, leftRejectedTables, rightRejectedTables);
                        rejectedJoins.Add(rejectedJoin);
                    }
                }
            }

            var rejectedJoinsByPredicate = rejectedJoins.GroupBy(r => r.Predicate);

            var issues = new List<CodeIssue>();

            foreach (var group in rejectedJoinsByPredicate)
            {
                var predicate = group.Key;
                var joins = group;

                var codeActions = new List<CodeAction>();

                // Include NULL values

                var tableInstances = joins.SelectMany(j => j.LeftRejectedTables.Concat(j.RightRejectedTables)).Distinct();
                var includeNullValuesCodeAction = new IncludeNullValuesCodeAction(predicate, semanticModel, tableInstances);
                codeActions.Add(includeNullValuesCodeAction);

                // Simplify joins

                var simplifyJoinCodeAction = new SimplifyJoinCodeAction(node.SyntaxTree, joins);
                codeActions.Add(simplifyJoinCodeAction);

                var description = "Expression rejects any NULL values introduced by OUTER JOIN";
                var codeIssue = new CodeIssue(CodeIssueKind.Warning, predicate.Span, description, codeActions);
                issues.Add(codeIssue);
            }

            var joinConditionWalker = new JoinConditionWalker(semanticModel, issues);
            joinConditionWalker.VisitFromClause(node.FromClause);

            return issues;
        }

        private static void SplitConjunctions(List<ExpressionSyntax> receiver, ExpressionSyntax expression)
        {
            if (expression == null)
                return;

            var stack = new Stack<ExpressionSyntax>();
            stack.Push(expression);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var binary = current as BinaryExpressionSyntax;
                if (binary != null && binary.Kind == SyntaxKind.LogicalAndExpression)
                {
                    stack.Push(binary.Left);
                    stack.Push(binary.Right);
                }
                else
                {
                    receiver.Add(current);
                }
            }
        }

        private sealed class JoinConditionWalker : SyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly List<CodeIssue> _codeIssues;
            private readonly Dictionary<TableInstanceSymbol, List<ExpressionSyntax>> _nullRejectedTables = new Dictionary<TableInstanceSymbol, List<ExpressionSyntax>>();

            public JoinConditionWalker(SemanticModel semanticModel, List<CodeIssue> codeIssues)
            {
                _semanticModel = semanticModel;
                _codeIssues = codeIssues;
            }

            private void AddNullRejectedTable(TableInstanceSymbol tableInstance, ExpressionSyntax conjunction)
            {
                List<ExpressionSyntax> conjunctions;
                if (!_nullRejectedTables.TryGetValue(tableInstance, out conjunctions))
                {
                    conjunctions = new List<ExpressionSyntax>();
                    _nullRejectedTables.Add(tableInstance, conjunctions);
                }

                conjunctions.Add(conjunction);
            }

            private void AddNullRejectedTables(
                ExpressionSyntax condition,
                SyntaxKind joinKind,
                ImmutableArray<TableInstanceSymbol> leftDeclaredTables,
                ImmutableArray<TableInstanceSymbol> rightDeclaredTables)
            {
                var conjunctions = new List<ExpressionSyntax>();
                SplitConjunctions(conjunctions, condition);

                foreach (var conjunction in conjunctions)
                {
                    // Check if we can derive from this conjunction that a table it depends on
                    // is null-rejected.

                    var tables = (from e in conjunction.DescendantNodes().OfType<ExpressionSyntax>()
                                  let c = _semanticModel.GetSymbol(e) as TableColumnInstanceSymbol
                                  where c != null
                                  select c.TableInstance).Distinct();

                    var nullRejectedTables = tables.Where(t => _semanticModel.IsRejectingNulls(conjunction, t));

                    foreach (var table in nullRejectedTables)
                    {
                        if (joinKind != SyntaxKind.LeftKeyword && leftDeclaredTables.Contains(table))
                        {
                            AddNullRejectedTable(table, conjunction);
                        }
                        else if (joinKind != SyntaxKind.RightKeyword && rightDeclaredTables.Contains(table))
                        {
                            AddNullRejectedTable(table, conjunction);
                        }
                    }
                }
            }

            private ImmutableArray<ExpressionSyntax> GetNullRejectingPredicates(ImmutableArray<TableInstanceSymbol> tableInstances)
            {
                return tableInstances.Where(t => _nullRejectedTables.ContainsKey(t))
                                     .SelectMany(t => _nullRejectedTables[t])
                                     .ToImmutableArray();
            }

            private void AddRejectedOuterJoin(OuterJoinedTableReferenceSyntax node, SyntaxKind newType, ImmutableArray<ExpressionSyntax> nullRejectingPredicates)
            {
                foreach (var nullRejectingPredicate in nullRejectingPredicates)
                {
                    var description = "Expression rejects any NULL values introduced by OUTER JOIN";
                    _codeIssues.Add(new CodeIssue(CodeIssueKind.Warning, nullRejectingPredicate.Span, description));
                }
            }

            public override void VisitInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
            {
                var leftDeclaredTables = _semanticModel.GetDeclaredSymbols(node.Left).ToImmutableArray();
                var rightDeclaredTables = _semanticModel.GetDeclaredSymbols(node.Left).ToImmutableArray();

                var joinKind = SyntaxKind.InnerKeyword;

                AddNullRejectedTables(node.Condition, joinKind, leftDeclaredTables, rightDeclaredTables);

                base.VisitInnerJoinedTableReference(node);
            }

            public override void VisitOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
            {
                // Get declared tables of left and right

                var leftDeclaredTables = _semanticModel.GetDeclaredSymbols(node.Left).ToImmutableArray();
                var rightDeclaredTables = _semanticModel.GetDeclaredSymbols(node.Right).ToImmutableArray();

                var joinKind = node.TypeKeyword.Kind;

                // Check for outer joins that should be replaced by left-/right-/inner joins

                if (joinKind == SyntaxKind.RightKeyword ||
                    joinKind == SyntaxKind.FullKeyword)
                {
                    var nullRejectingPredicates = GetNullRejectingPredicates(leftDeclaredTables);
                    if (nullRejectingPredicates.Any())
                    {
                        var newType = joinKind == SyntaxKind.RightKeyword
                            ? SyntaxKind.InnerKeyword
                            : SyntaxKind.LeftKeyword;

                        AddRejectedOuterJoin(node, newType, nullRejectingPredicates);
                    }
                }

                if (joinKind == SyntaxKind.LeftKeyword ||
                    joinKind == SyntaxKind.FullKeyword)
                {
                    var nullRejectingPredicates = GetNullRejectingPredicates(rightDeclaredTables);
                    if (nullRejectingPredicates.Any())
                    {
                        var newType = joinKind == SyntaxKind.LeftKeyword
                            ? SyntaxKind.InnerKeyword
                            : SyntaxKind.RightKeyword;

                        AddRejectedOuterJoin(node, newType, nullRejectingPredicates);
                    }
                }

                if (joinKind != SyntaxKind.FullKeyword)
                    AddNullRejectedTables(node.Condition, joinKind, leftDeclaredTables, rightDeclaredTables);

                base.VisitOuterJoinedTableReference(node);
            }

            public override void VisitDerivedTableReference(DerivedTableReferenceSyntax node)
            {
                // Don't visit children.
            }
        }

        private sealed class RejectedOuterJoin
        {
            public RejectedOuterJoin(ExpressionSyntax predicate, OuterJoinedTableReferenceSyntax join, ImmutableArray<TableInstanceSymbol> leftRejectedTables, ImmutableArray<TableInstanceSymbol> rightRejectedTables)
            {
                Join = join;
                Predicate = predicate;
                LeftRejectedTables = leftRejectedTables;
                RightRejectedTables = rightRejectedTables;
            }

            public OuterJoinedTableReferenceSyntax Join { get; }
            public ExpressionSyntax Predicate { get; }
            public ImmutableArray<TableInstanceSymbol> LeftRejectedTables { get; }
            public ImmutableArray<TableInstanceSymbol> RightRejectedTables { get; }
        }

        private sealed class SimplifyJoinCodeAction : CodeAction
        {
            private readonly ImmutableArray<RejectedOuterJoin> _rejectedOuterJoins;

            public SimplifyJoinCodeAction(SyntaxTree syntaxTree, IEnumerable<RejectedOuterJoin> rejectedOuterJoins)
                : base(syntaxTree)
            {
                _rejectedOuterJoins = rejectedOuterJoins.ToImmutableArray();
            }

            public override string Description => "Simplify outer join";

            protected override void GetChanges(TextChangeSet changeSet)
            {
                foreach (var rejectedOuterJoin in _rejectedOuterJoins)
                {
                    var span = rejectedOuterJoin.Join.TypeKeyword.Span;
                    var newKeyword = GetNewKeyword(rejectedOuterJoin).GetText();
                    changeSet.ReplaceText(span, newKeyword);
                }
            }

            private static SyntaxKind GetNewKeyword(RejectedOuterJoin rejectedOuterJoin)
            {
                var rejectsLeft = rejectedOuterJoin.LeftRejectedTables.Any();
                var rejectsRight = rejectedOuterJoin.RightRejectedTables.Any();
                var typeKeyword = rejectedOuterJoin.Join.TypeKeyword.Kind;

                if (typeKeyword == SyntaxKind.FullKeyword)
                {
                    if (rejectsLeft && rejectsRight)
                        return SyntaxKind.InnerKeyword;
                    if (rejectsLeft)
                        return SyntaxKind.LeftKeyword;
                    return SyntaxKind.RightKeyword;
                }

                return SyntaxKind.InnerKeyword;
            }
        }

        private sealed class IncludeNullValuesCodeAction : CodeAction
        {
            private readonly ExpressionSyntax _predicate;
            private readonly SemanticModel _semanticModel;
            private readonly IEnumerable<TableInstanceSymbol> _tables;

            public IncludeNullValuesCodeAction(ExpressionSyntax predicate, SemanticModel semanticModel, IEnumerable<TableInstanceSymbol> tables)
                : base(predicate.SyntaxTree)
            {
                _predicate = predicate;
                _semanticModel = semanticModel;
                _tables = tables;
            }

            public override string Description => "Include NULL values";

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var text = _predicate.SyntaxTree.Text;
                var columnExpressions = (from e in _predicate.DescendantNodes().OfType<ExpressionSyntax>()
                                         let c = _semanticModel.GetSymbol(e) as TableColumnInstanceSymbol
                                         where c != null && _tables.Contains(c.TableInstance)
                                         select text.GetText(e.Span)).Distinct();

                var isNullChecks = string.Concat(columnExpressions.Select(e => $" OR {e} IS NULL"));

                changeSet.InsertText(_predicate.Span.Start, "(");
                changeSet.InsertText(_predicate.Span.End, isNullChecks + ")");
            }
        }
    }
}