#nullable enable

using System.Collections.Immutable;
using System.Diagnostics;

using NQuery.Binding;
using NQuery.Symbols.Aggregation;

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

            // The binder never emits a probe or a passthru predicate -- those exist only in
            // the legacy SubqueryExpander's output, which the new pipeline doesn't run. So
            // PassthruPredicate is always null here and can't contain a subquery; algebrize
            // it defensively (a no-op on null) and assert the invariant.
            var passthruApplies = new List<PendingApply>();
            var passthruPredicate = AlgebrizeExpressionOrNull(node.PassthruPredicate, passthruApplies);
            Debug.Assert(passthruApplies.Count == 0, "The binder does not produce passthru predicates, let alone ones containing subqueries.");

            // Split the ON condition into conjuncts. A subquery-free conjunct stays as the
            // join's own condition (so an equi-join can still become a hash match). A
            // conjunct that introduces a subquery turns into an Apply, which can't live
            // inside the join expression -- so it is hoisted into a filter above the join,
            // where the Apply (correlated to the join's whole left ++ right output)
            // produces the probe/value slot the filter then tests.
            var joinConditions = new List<LogicalExpression>();
            var filterConditions = new List<LogicalExpression>();
            var applies = new List<PendingApply>();

            foreach (var conjunct in SplitConjuncts(node.Condition))
            {
                var conjunctApplies = new List<PendingApply>();
                var algebrized = AlgebrizeExpression(conjunct, conjunctApplies);
                if (conjunctApplies.Count == 0)
                {
                    joinConditions.Add(algebrized);
                }
                else
                {
                    applies.AddRange(conjunctApplies);
                    filterConditions.Add(algebrized);
                }
            }

            var conditions = joinConditions.ToImmutableArray();

            // Normalize RIGHT OUTER to LEFT OUTER by swapping inputs, so the logical layer
            // never sees a right-outer join. The conditions reference slots by identity, so
            // swapping the operands preserves meaning.
            var join = node.JoinType == BoundJoinType.RightOuter
                ? new LogicalJoin(LogicalJoinKind.LeftOuter, right, left, conditions, node.Probe, passthruPredicate)
                : new LogicalJoin(MapJoinKind(node.JoinType), left, right, conditions, node.Probe, passthruPredicate);

            if (applies.Count == 0)
                return join;

            // Hoisting a conjunct out of the ON clause into a filter above the join only
            // preserves semantics for an inner join, where ON is a plain filter. For an
            // outer/semi/anti join it would change which rows are preserved, so that case
            // isn't lowered.
            //
            // TODO: Lower this. A LEFT OUTER ON p can be rewritten as the matching pairs
            //       (sigma_p of the cross product, where the Apply works as for an inner
            //       join) unioned with the unmatched left rows null-padded -- the same
            //       shape as the FULL OUTER expansion in the planner. Semi/anti are similar.
            if (node.JoinType != BoundJoinType.Inner)
                throw new NotSupportedException("Subqueries inside a non-inner join's ON condition are not yet supported.");

            var withApplies = WrapInApplies(join, applies);
            return new LogicalFilter(withApplies, filterConditions.ToImmutableArray());
        }

        // Splits a bound predicate into its top-level conjuncts (an AND chain). Used to
        // keep subquery-free join conditions on the join while pulling subquery-bearing
        // ones above it.
        private static IEnumerable<BoundExpression> SplitConjuncts(BoundExpression? expression)
        {
            if (expression is null)
                yield break;

            if (expression is BoundBinaryExpression { OperatorKind: BinaryOperatorKind.LogicalAnd } binary)
            {
                foreach (var conjunct in SplitConjuncts(binary.Left))
                    yield return conjunct;
                foreach (var conjunct in SplitConjuncts(binary.Right))
                    yield return conjunct;
            }
            else
            {
                yield return expression;
            }
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

            // A scalar subquery must yield at most one row. When the relation already
            // guarantees that, attach it directly. Otherwise guard it with a cardinality
            // check. Modelling the guarantee as an aggregate (rather than an opaque
            // max-one-row operator) keeps the subquery in the form the apply-decorrelation
            // rules understand.
            if (ReturnsAtMostOneRow(relation))
            {
                applies.Add(new PendingApply(LogicalApplyKind.LeftOuter, relation, probe: null));
                return new LogicalValueSlotExpression(node.Value);
            }

            var guarded = GuardSingleRow(relation, node.Value, out var output);
            applies.Add(new PendingApply(LogicalApplyKind.LeftOuter, guarded, probe: null));
            return new LogicalValueSlotExpression(output);
        }

        // ANY(value) collapses the (zero or one) surviving rows to the scalar result;
        // COUNT detects the error case. With no GROUP BY the aggregate always yields a
        // single row, so the assert sees one row carrying both the value and the count.
        //
        // TODO: This aggregate form is canonical so the apply over it can be unnested,
        //       but ApplyPushdown doesn't do that yet (see its GroupBy TODO). Until then a
        //       correlated guarded subquery executes as correlated nested loops.
        private LogicalOperator GuardSingleRow(LogicalOperator relation, ValueSlot value, out ValueSlot output)
        {
            var anyOutput = _valueSlotFactory.CreateTemporary(value.Type);
            var countOutput = _valueSlotFactory.CreateTemporary(typeof(int));

            var any = new LogicalAggregatedValue(
                anyOutput,
                BuiltInAggregates.Any,
                BuiltInAggregates.Any.Definition.CreateAggregatable(value.Type),
                new LogicalValueSlotExpression(value));

            var count = new LogicalAggregatedValue(
                countOutput,
                BuiltInAggregates.Count,
                BuiltInAggregates.Count.Definition.CreateAggregatable(typeof(int)),
                new LogicalLiteralExpression(0));

            var aggregate = new LogicalAggregate(relation, ImmutableArray<BoundComparedValue>.Empty, ImmutableArray.Create(any, count));

            var condition = Binary(new LogicalValueSlotExpression(countOutput), BinaryOperatorKind.LessOrEqual, new LogicalLiteralExpression(1));

            output = anyOutput;
            return new LogicalAssert(aggregate, condition, Resources.SubqueryReturnedMoreThanRow);
        }

        // Whether a relation provably returns at most one row, so the cardinality guard
        // can be skipped. Conservative: it peels cardinality-non-increasing unary
        // operators down to a capping node (a scalar aggregate, TOP 1, or a constant).
        private static bool ReturnsAtMostOneRow(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                case LogicalOperatorKind.Constant:
                    return true;
                case LogicalOperatorKind.Aggregate:
                    return ((LogicalAggregate)node).Groups.IsEmpty;
                case LogicalOperatorKind.Top:
                    var top = (LogicalTop)node;
                    // TOP 1 caps at one row, unless WITH TIES can admit more.
                    return top.Limit <= 1 && top.TieEntries.IsEmpty || ReturnsAtMostOneRow(top.Input);
                case LogicalOperatorKind.Filter:
                    return ReturnsAtMostOneRow(((LogicalFilter)node).Input);
                case LogicalOperatorKind.Compute:
                    return ReturnsAtMostOneRow(((LogicalCompute)node).Input);
                case LogicalOperatorKind.Project:
                    return ReturnsAtMostOneRow(((LogicalProject)node).Input);
                case LogicalOperatorKind.Sort:
                    return ReturnsAtMostOneRow(((LogicalSort)node).Input);
                case LogicalOperatorKind.Assert:
                    return ReturnsAtMostOneRow(((LogicalAssert)node).Input);
                default:
                    return false;
            }
        }

        private static LogicalBinaryExpression Binary(LogicalExpression left, BinaryOperatorKind kind, LogicalExpression right)
        {
            var result = BinaryOperator.Resolve(kind, left.Type, right.Type);
            return new LogicalBinaryExpression(left, kind, result, right);
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
