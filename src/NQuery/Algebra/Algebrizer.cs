#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Algebra
{
    // Translates the binder's relational bound tree into the logical algebra
    // (LogicalOperator). This is the Bound -> Logical lowering.
    //
    // It is layered on top of the existing bound tree -- it reuses the same
    // ValueSlots (and resolved operator/symbol metadata) and does not modify the
    // binder or the bound nodes. The result is fully logical:
    //
    //   * column references become slot references;
    //   * correlation is made explicit -- a scalar subquery becomes a LeftOuter
    //     Apply whose value slot the expression then references, and an EXISTS
    //     subquery becomes a Semi Apply producing a boolean probe slot the
    //     expression references. The logical expression language therefore cannot
    //     contain a relation: subqueries are never expression nodes.
    //
    // Apply introduction happens here because it *is* part of forming the algebra.
    // Decorrelation -- pushing those applies down until the right no longer depends
    // on the left, turning them into ordinary joins -- is a separate optimizer pass.
    //
    // Out of scope: subqueries inside join conditions (an apply has a single input
    // to attach to, but a join condition sees both sides) -- these throw. And
    // optimizer-introduced physical nodes (hash match, stream aggregate,
    // concatenation, table spools) never appear in the binder's output.
    internal sealed class Algebrizer
    {
        private readonly ValueSlotFactory _valueSlotFactory = new();

        public static LogicalQuery Algebrize(BoundQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var algebrizer = new Algebrizer();
            var root = algebrizer.AlgebrizeRelation(query.Relation);
            return new LogicalQuery(root, query.OutputColumns);
        }

        public static LogicalOperator Algebrize(BoundRelation relation)
        {
            ArgumentNullException.ThrowIfNull(relation);

            return new Algebrizer().AlgebrizeRelation(relation);
        }

        private LogicalOperator AlgebrizeRelation(BoundRelation node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.EmptyRelation:
                    return AlgebrizeEmptyRelation((BoundEmptyRelation)node);
                case BoundNodeKind.ConstantRelation:
                    return AlgebrizeConstantRelation((BoundConstantRelation)node);
                case BoundNodeKind.TableRelation:
                    return AlgebrizeTableRelation((BoundTableRelation)node);
                case BoundNodeKind.DerivedTableRelation:
                    return AlgebrizeDerivedTableRelation((BoundDerivedTableRelation)node);
                case BoundNodeKind.FilterRelation:
                    return AlgebrizeFilterRelation((BoundFilterRelation)node);
                case BoundNodeKind.ComputeRelation:
                    return AlgebrizeComputeRelation((BoundComputeRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return AlgebrizeProjectRelation((BoundProjectRelation)node);
                case BoundNodeKind.JoinRelation:
                    return AlgebrizeJoinRelation((BoundJoinRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return AlgebrizeGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.UnionRelation:
                    return AlgebrizeUnionRelation((BoundUnionRelation)node);
                case BoundNodeKind.IntersectOrExceptRelation:
                    return AlgebrizeIntersectOrExceptRelation((BoundIntersectOrExceptRelation)node);
                case BoundNodeKind.SortRelation:
                    return AlgebrizeSortRelation((BoundSortRelation)node);
                case BoundNodeKind.TopRelation:
                    return AlgebrizeTopRelation((BoundTopRelation)node);
                case BoundNodeKind.AssertRelation:
                    return AlgebrizeAssertRelation((BoundAssertRelation)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private LogicalOperator AlgebrizeEmptyRelation(BoundEmptyRelation node)
        {
            return new LogicalEmpty();
        }

        private LogicalOperator AlgebrizeConstantRelation(BoundConstantRelation node)
        {
            return new LogicalConstant();
        }

        private LogicalOperator AlgebrizeTableRelation(BoundTableRelation node)
        {
            return new LogicalTableScan(node.TableInstance, node.DefinedValues);
        }

        private LogicalOperator AlgebrizeDerivedTableRelation(BoundDerivedTableRelation node)
        {
            // A derived table is a pure scoping wrapper: its output slots are the
            // inner relation's slots, so it carries no algebraic meaning -- inline
            // it rather than introduce a node that would only be removed later.
            return AlgebrizeRelation(node.Relation);
        }

        private LogicalOperator AlgebrizeFilterRelation(BoundFilterRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var applies = new List<PendingApply>();
            var condition = AlgebrizeExpression(node.Condition, applies);
            input = WrapInApplies(input, applies);
            var conditions = FlattenConjuncts(condition).ToImmutableArray();
            return new LogicalFilter(input, conditions);
        }

        private LogicalOperator AlgebrizeComputeRelation(BoundComputeRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var applies = new List<PendingApply>();
            var definedValues = node.DefinedValues.Select(v => AlgebrizeComputedValue(v, applies)).ToImmutableArray();
            input = WrapInApplies(input, applies);
            return new LogicalCompute(input, definedValues);
        }

        private LogicalOperator AlgebrizeProjectRelation(BoundProjectRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            return new LogicalProject(input, node.Outputs);
        }

        private LogicalOperator AlgebrizeJoinRelation(BoundJoinRelation node)
        {
            var left = AlgebrizeRelation(node.Left);
            var right = AlgebrizeRelation(node.Right);

            var applies = new List<PendingApply>();
            var condition = AlgebrizeExpressionOrNull(node.Condition, applies);
            var passthruPredicate = AlgebrizeExpressionOrNull(node.PassthruPredicate, applies);
            if (applies.Count > 0)
                throw new NotSupportedException("Subqueries inside join conditions are not yet supported by Apply introduction.");

            var conditions = FlattenConjuncts(condition).ToImmutableArray();

            // Normalize RIGHT OUTER to LEFT OUTER by swapping inputs, so the logical
            // layer never sees a right-outer join. The condition/passthru reference
            // slots by identity, so swapping the operands preserves meaning.
            if (node.JoinType == BoundJoinType.RightOuter)
                return new LogicalJoin(LogicalJoinKind.LeftOuter, right, left, conditions, node.Probe, passthruPredicate);

            return new LogicalJoin(MapJoinKind(node.JoinType), left, right, conditions, node.Probe, passthruPredicate);
        }

        private static LogicalJoinKind MapJoinKind(BoundJoinType joinType)
        {
            return joinType switch
            {
                BoundJoinType.Inner => LogicalJoinKind.Inner,
                BoundJoinType.LeftOuter => LogicalJoinKind.LeftOuter,
                BoundJoinType.FullOuter => LogicalJoinKind.FullOuter,
                BoundJoinType.LeftSemi => LogicalJoinKind.LeftSemi,
                BoundJoinType.LeftAntiSemi => LogicalJoinKind.LeftAntiSemi,
                _ => throw ExceptionBuilder.UnexpectedValue(joinType)
            };
        }

        // Splits a predicate into its top-level conjuncts so that Filter/Join can
        // store them as a list (an implicit AND), which is what selection pushdown
        // moves individually.
        private static IEnumerable<LogicalExpression> FlattenConjuncts(LogicalExpression? expression)
        {
            if (expression is null)
                yield break;

            if (expression is LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.LogicalAnd } binary)
            {
                foreach (var conjunct in FlattenConjuncts(binary.Left))
                    yield return conjunct;
                foreach (var conjunct in FlattenConjuncts(binary.Right))
                    yield return conjunct;
            }
            else
            {
                yield return expression;
            }
        }

        private LogicalOperator AlgebrizeGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var applies = new List<PendingApply>();
            var aggregates = node.Aggregates.Select(a => AlgebrizeAggregatedValue(a, applies)).ToImmutableArray();
            input = WrapInApplies(input, applies);
            return new LogicalAggregate(input, node.Groups, aggregates);
        }

        private LogicalOperator AlgebrizeUnionRelation(BoundUnionRelation node)
        {
            var inputs = node.Inputs.Select(AlgebrizeRelation).ToImmutableArray();
            return new LogicalUnion(node.IsUnionAll, inputs, node.DefinedValues, node.Comparers);
        }

        private LogicalOperator AlgebrizeIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            var left = AlgebrizeRelation(node.Left);
            var right = AlgebrizeRelation(node.Right);
            return new LogicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
        }

        private LogicalOperator AlgebrizeSortRelation(BoundSortRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            return new LogicalSort(node.IsDistinct, input, node.SortedValues);
        }

        private LogicalOperator AlgebrizeTopRelation(BoundTopRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            return new LogicalTop(input, node.Limit, node.TieEntries);
        }

        private LogicalOperator AlgebrizeAssertRelation(BoundAssertRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var applies = new List<PendingApply>();
            var condition = AlgebrizeExpression(node.Condition, applies);
            input = WrapInApplies(input, applies);
            return new LogicalAssert(input, condition, node.Message);
        }

        private LogicalComputedValue AlgebrizeComputedValue(BoundComputedValue value, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(value.Expression, applies);
            return new LogicalComputedValue(expression, value.ValueSlot);
        }

        private LogicalAggregatedValue AlgebrizeAggregatedValue(BoundAggregatedValue value, List<PendingApply> applies)
        {
            var argument = AlgebrizeExpression(value.Argument, applies);
            return new LogicalAggregatedValue(value.Output, value.Aggregate, value.Aggregatable, argument);
        }

        // Each operator collects the applies produced while translating its own
        // expressions, then wraps its input in them so the subquery slots are in
        // scope for the expressions that reference them.
        private static LogicalOperator WrapInApplies(LogicalOperator input, List<PendingApply> applies)
        {
            foreach (var apply in applies)
                input = new LogicalApply(apply.Kind, input, apply.Relation, apply.Probe);
            return input;
        }

        private LogicalExpression? AlgebrizeExpressionOrNull(BoundExpression? node, List<PendingApply> applies)
        {
            return node is null ? null : AlgebrizeExpression(node, applies);
        }

        private LogicalExpression AlgebrizeExpression(BoundExpression node, List<PendingApply> applies)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return AlgebrizeLiteralExpression((BoundLiteralExpression)node, applies);
                case BoundNodeKind.ValueSlotExpression:
                    return AlgebrizeValueSlotExpression((BoundValueSlotExpression)node, applies);
                case BoundNodeKind.ColumnExpression:
                    return AlgebrizeColumnExpression((BoundColumnExpression)node, applies);
                case BoundNodeKind.VariableExpression:
                    return AlgebrizeVariableExpression((BoundVariableExpression)node, applies);
                case BoundNodeKind.UnaryExpression:
                    return AlgebrizeUnaryExpression((BoundUnaryExpression)node, applies);
                case BoundNodeKind.BinaryExpression:
                    return AlgebrizeBinaryExpression((BoundBinaryExpression)node, applies);
                case BoundNodeKind.ConversionExpression:
                    return AlgebrizeConversionExpression((BoundConversionExpression)node, applies);
                case BoundNodeKind.IsNullExpression:
                    return AlgebrizeIsNullExpression((BoundIsNullExpression)node, applies);
                case BoundNodeKind.CaseExpression:
                    return AlgebrizeCaseExpression((BoundCaseExpression)node, applies);
                case BoundNodeKind.FunctionInvocationExpression:
                    return AlgebrizeFunctionInvocationExpression((BoundFunctionInvocationExpression)node, applies);
                case BoundNodeKind.PropertyAccessExpression:
                    return AlgebrizePropertyAccessExpression((BoundPropertyAccessExpression)node, applies);
                case BoundNodeKind.MethodInvocationExpression:
                    return AlgebrizeMethodInvocationExpression((BoundMethodInvocationExpression)node, applies);
                case BoundNodeKind.SingleRowSubselect:
                    return AlgebrizeSingleRowSubselect((BoundSingleRowSubselect)node, applies);
                case BoundNodeKind.ExistsSubselect:
                    return AlgebrizeExistsSubselect((BoundExistsSubselect)node, applies);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private LogicalExpression AlgebrizeLiteralExpression(BoundLiteralExpression node, List<PendingApply> applies)
        {
            return new LogicalLiteralExpression(node.Value);
        }

        private LogicalExpression AlgebrizeValueSlotExpression(BoundValueSlotExpression node, List<PendingApply> applies)
        {
            return new LogicalValueSlotExpression(node.ValueSlot);
        }

        // A column reference resolves to a reference to the slot its symbol owns:
        // this is the "symbol-refs become slot-refs" step of algebrization.
        private LogicalExpression AlgebrizeColumnExpression(BoundColumnExpression node, List<PendingApply> applies)
        {
            return new LogicalValueSlotExpression(node.Symbol.ValueSlot);
        }

        private LogicalExpression AlgebrizeVariableExpression(BoundVariableExpression node, List<PendingApply> applies)
        {
            return new LogicalVariableExpression(node.Symbol);
        }

        private LogicalExpression AlgebrizeUnaryExpression(BoundUnaryExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalUnaryExpression(node.OperatorKind, node.Result, expression);
        }

        private LogicalExpression AlgebrizeBinaryExpression(BoundBinaryExpression node, List<PendingApply> applies)
        {
            var left = AlgebrizeExpression(node.Left, applies);
            var right = AlgebrizeExpression(node.Right, applies);
            return new LogicalBinaryExpression(left, node.OperatorKind, node.Result, right);
        }

        private LogicalExpression AlgebrizeConversionExpression(BoundConversionExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalConversionExpression(expression, node.Type, node.Conversion);
        }

        private LogicalExpression AlgebrizeIsNullExpression(BoundIsNullExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalIsNullExpression(expression);
        }

        private LogicalExpression AlgebrizeCaseExpression(BoundCaseExpression node, List<PendingApply> applies)
        {
            var caseLabels = node.CaseLabels.Select(l => AlgebrizeCaseLabel(l, applies)).ToImmutableArray();
            var elseExpression = AlgebrizeExpressionOrNull(node.ElseExpression, applies);
            return new LogicalCaseExpression(caseLabels, elseExpression);
        }

        private LogicalCaseLabel AlgebrizeCaseLabel(BoundCaseLabel label, List<PendingApply> applies)
        {
            var condition = AlgebrizeExpression(label.Condition, applies);
            var thenExpression = AlgebrizeExpression(label.ThenExpression, applies);
            return new LogicalCaseLabel(condition, thenExpression);
        }

        private LogicalExpression AlgebrizeFunctionInvocationExpression(BoundFunctionInvocationExpression node, List<PendingApply> applies)
        {
            var arguments = node.Arguments.Select(a => AlgebrizeExpression(a, applies)).ToImmutableArray();
            return new LogicalFunctionInvocationExpression(arguments, node.Result);
        }

        private LogicalExpression AlgebrizePropertyAccessExpression(BoundPropertyAccessExpression node, List<PendingApply> applies)
        {
            var target = AlgebrizeExpression(node.Target, applies);
            return new LogicalPropertyAccessExpression(target, node.Symbol);
        }

        private LogicalExpression AlgebrizeMethodInvocationExpression(BoundMethodInvocationExpression node, List<PendingApply> applies)
        {
            var target = AlgebrizeExpression(node.Target, applies);
            var arguments = node.Arguments.Select(a => AlgebrizeExpression(a, applies)).ToImmutableArray();
            return new LogicalMethodInvocationExpression(target, arguments, node.Result);
        }

        // A scalar subquery becomes a LeftOuter apply that exposes its value slot;
        // the expression is replaced by a reference to that slot.
        private LogicalExpression AlgebrizeSingleRowSubselect(BoundSingleRowSubselect node, List<PendingApply> applies)
        {
            var relation = AlgebrizeRelation(node.Relation);
            applies.Add(new PendingApply(LogicalApplyKind.LeftOuter, relation, probe: null));
            return new LogicalValueSlotExpression(node.Value);
        }

        // EXISTS becomes a Semi apply that produces a boolean probe slot; the
        // expression is replaced by a reference to the probe (so NOT EXISTS is just
        // NOT <probe>, and EXISTS can appear anywhere in a predicate).
        private LogicalExpression AlgebrizeExistsSubselect(BoundExistsSubselect node, List<PendingApply> applies)
        {
            var relation = AlgebrizeRelation(node.Relation);
            var probe = _valueSlotFactory.CreateTemporary(typeof(bool));
            applies.Add(new PendingApply(LogicalApplyKind.LeftSemi, relation, probe));
            return new LogicalValueSlotExpression(probe);
        }

        private readonly struct PendingApply
        {
            public PendingApply(LogicalApplyKind kind, LogicalOperator relation, ValueSlot? probe)
            {
                Kind = kind;
                Relation = relation;
                Probe = probe;
            }

            public LogicalApplyKind Kind { get; }

            public LogicalOperator Relation { get; }

            public ValueSlot? Probe { get; }
        }
    }
}
