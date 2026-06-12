using System.Collections.Immutable;

using NQuery.Symbols;
using NQuery.Syntax;

using NQuery.Binding;

namespace NQuery.Refactor.Binding
{
    // Walks the recursive member of a recursive CTE (now a syntax-shaped BoundQuery tree rather
    // than a relational tree) and reports the constructs that aren't allowed in a recursive
    // member: UNION (other than the recursive UNION ALL itself, which is handled elsewhere),
    // DISTINCT, TOP, GROUP BY/aggregates, outer joins, and recursive references that appear more
    // than once or inside a subquery.
    internal sealed class RecursiveCommonTableExpressionChecker
    {
        private readonly CommonTableExpressionSyntax _syntax;
        private readonly List<Diagnostic> _diagnostics;
        private readonly CommonTableExpressionSymbol _symbol;

        private int _subqueryCounter;
        private bool _hasSeenRecursiveReference;

        public RecursiveCommonTableExpressionChecker(CommonTableExpressionSyntax syntax, List<Diagnostic> diagnostics, CommonTableExpressionSymbol symbol)
        {
            _syntax = syntax;
            _diagnostics = diagnostics;
            _symbol = symbol;
        }

        public void Check(BoundQuery query)
        {
            WalkQuery(query);
        }

        private void WalkQuery(BoundQuery node)
        {
            switch (node)
            {
                case BoundSelectQuery select:
                    WalkSelectQuery(select);
                    break;
                case BoundUnionQuery union:
                    _diagnostics.ReportCteContainsUnion(_syntax.Name);
                    foreach (var input in union.Inputs)
                        WalkQuery(input.Query);
                    break;
                case BoundIntersectOrExceptQuery intersectOrExcept:
                    WalkQuery(intersectOrExcept.Left.Query);
                    WalkQuery(intersectOrExcept.Right.Query);
                    break;
                case BoundOrderedQuery ordered:
                    WalkQuery(ordered.Query);
                    break;
                case BoundEmptyQuery:
                    break;
            }
        }

        private void WalkSelectQuery(BoundSelectQuery node)
        {
            if (node.Top is not null)
                _diagnostics.ReportCteContainsTop(_syntax.Name);

            if (node.Select.IsDistinct)
                _diagnostics.ReportCteContainsDistinct(_syntax.Name);

            if (node.GroupBy is not null || !node.Select.Aggregates.IsEmpty)
                _diagnostics.ReportCteContainsGroupByHavingOrAggregate(_syntax.Name);

            if (node.From is not null)
                WalkTableReference(node.From.Root);

            if (node.Where is not null)
                WalkExpression(node.Where.Condition);

            if (node.Having is not null)
                WalkExpression(node.Having.Condition);

            foreach (var projection in node.Select.Projections)
                WalkExpression(projection.Expression);

            if (node.GroupBy is not null)
            {
                foreach (var computedGroup in node.GroupBy.ComputedGroups)
                    WalkExpression(computedGroup.Expression);
            }

            foreach (var aggregate in node.Select.Aggregates)
                WalkExpression(aggregate.Argument);
        }

        private void WalkTableReference(BoundTableReference node)
        {
            switch (node)
            {
                case BoundNamedTableReference named:
                    var hasRecursiveReference = named.TableInstance.Table == _symbol;
                    if (_subqueryCounter > 0 && hasRecursiveReference)
                        _diagnostics.ReportCteContainsRecursiveReferenceInSubquery(_syntax.Name);
                    if (_hasSeenRecursiveReference && hasRecursiveReference)
                        _diagnostics.ReportCteContainsMultipleRecursiveReferences(_syntax.Name);
                    if (hasRecursiveReference)
                        _hasSeenRecursiveReference = true;
                    break;
                case BoundDerivedTableReference:
                    // Legacy behaviour: don't descend into derived tables.
                    break;
                case BoundJoinTableReference join:
                    if (join.JoinType is BoundJoinType.FullOuter or BoundJoinType.LeftOuter or BoundJoinType.RightOuter)
                        _diagnostics.ReportCteContainsOuterJoin(_syntax.Name);
                    WalkTableReference(join.Left);
                    WalkTableReference(join.Right);
                    if (join.Condition is not null)
                        WalkExpression(join.Condition);
                    break;
            }
        }

        private void WalkExpression(BoundExpression node)
        {
            switch (node)
            {
                case BoundUnaryExpression unary:
                    WalkExpression(unary.Expression);
                    break;
                case BoundBinaryExpression binary:
                    WalkExpression(binary.Left);
                    WalkExpression(binary.Right);
                    break;
                case BoundConversionExpression conversion:
                    WalkExpression(conversion.Expression);
                    break;
                case BoundIsNullExpression isNull:
                    WalkExpression(isNull.Expression);
                    break;
                case BoundCaseExpression caseExpression:
                    foreach (var label in caseExpression.CaseLabels)
                    {
                        WalkExpression(label.Condition);
                        WalkExpression(label.ThenExpression);
                    }
                    if (caseExpression.ElseExpression is not null)
                        WalkExpression(caseExpression.ElseExpression);
                    break;
                case BoundFunctionInvocationExpression function:
                    foreach (var argument in function.Arguments)
                        WalkExpression(argument);
                    break;
                case BoundAggregateExpression aggregate:
                    WalkExpression(aggregate.Argument);
                    break;
                case BoundPropertyAccessExpression propertyAccess:
                    WalkExpression(propertyAccess.Target);
                    break;
                case BoundMethodInvocationExpression method:
                    WalkExpression(method.Target);
                    foreach (var argument in method.Arguments)
                        WalkExpression(argument);
                    break;
                case BoundSingleRowSubselect singleRow:
                    _subqueryCounter++;
                    WalkQuery(singleRow.Query);
                    _subqueryCounter--;
                    break;
                case BoundExistsSubselect exists:
                    _subqueryCounter++;
                    WalkQuery(exists.Query);
                    _subqueryCounter--;
                    break;
            }
        }
    }
}
