#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // A nested-loops join. The condition and passthru predicate are compiled once,
    // against the combined (left ++ right) slot layout, and take the row buffer as a
    // parameter -- so the executable join is reusable across CreateIterator() calls.
    internal sealed class ExecutableNestedLoops : ExecutableOperator
    {
        private static readonly EmittedPredicate AlwaysTrue = _ => true;
        private static readonly EmittedPredicate AlwaysFalse = _ => false;

        private readonly ExecutableOperator _left;
        private readonly ExecutableOperator _right;
        private readonly LogicalJoinKind _joinKind;
        private readonly ValueSlot? _probe;
        private readonly EmittedPredicate _predicate;
        private readonly EmittedPredicate _passthruPredicate;

        public ExecutableNestedLoops(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right, LogicalJoinKind joinKind, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate)
            : base(outputValueSlots)
        {
            _left = left;
            _right = right;
            _joinKind = joinKind;
            _probe = probe;

            // The predicates see both sides, so compile them against the combined slot
            // layout; that is exactly the order of the CombinedRowBuffer the iterators
            // feed them at run time.
            var combined = left.OutputValueSlots.AddRange(right.OutputValueSlots);
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(combined);
            _predicate = CompileConjunction(conditions, slotIndices);
            _passthruPredicate = passthruPredicate is null
                ? AlwaysFalse
                : EmittedExpressionCompiler.CompilePredicate(passthruPredicate, slotIndices);
        }

        public override Iterator CreateIterator()
        {
            var left = _left.CreateIterator();
            var right = _right.CreateIterator();

            switch (_joinKind)
            {
                case LogicalJoinKind.Inner:
                    return new InnerNestedLoopsIterator(left, right, _predicate, _passthruPredicate);
                case LogicalJoinKind.LeftOuter:
                    return new LeftOuterNestedLoopsIterator(left, right, _predicate, _passthruPredicate);
                case LogicalJoinKind.LeftSemi:
                    return _probe is null
                        ? new LeftSemiNestedLoopsIterator(left, right, _predicate, _passthruPredicate)
                        : new ProbingLeftSemiNestedLoopsIterator(left, right, _predicate);
                case LogicalJoinKind.LeftAntiSemi:
                    return new LeftAntiSemiNestedLoopsIterator(left, right, _predicate, _passthruPredicate);
                case LogicalJoinKind.FullOuter:
                    throw new NotSupportedException("FULL OUTER JOIN requires a hash match; nested loops cannot produce it.");
                default:
                    throw ExceptionBuilder.UnexpectedValue(_joinKind);
            }
        }

        // An empty condition list means an unconditional (cross) join, so the
        // predicate is constant true. Each conjunct already yields false on NULL.
        private static EmittedPredicate CompileConjunction(ImmutableArray<LogicalExpression> conditions, IReadOnlyDictionary<ValueSlot, int> slotIndices)
        {
            if (conditions.IsEmpty)
                return AlwaysTrue;

            var predicates = conditions
                             .Select(c => EmittedExpressionCompiler.CompilePredicate(c, slotIndices))
                             .ToImmutableArray();

            if (predicates.Length == 1)
                return predicates[0];

            return rowBuffer =>
            {
                foreach (var predicate in predicates)
                {
                    if (!predicate(rowBuffer))
                        return false;
                }

                return true;
            };
        }
    }
}
