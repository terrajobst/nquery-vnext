#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // A correlated apply (dependent join) executed as nested loops: for each left row
    // the right subtree is re-opened and re-evaluated. The apply has no join predicate
    // of its own -- the correlation lives inside the right subtree (a correlated filter
    // / compute reading the outer row) -- so the nested-loops iterators run with a
    // constant-true predicate.
    //
    // The outer references are wired by handing the right subtree an outer buffer: the
    // left iterator's (live) row buffer, prepended to any outer the apply itself
    // received. Because the loop keeps the left positioned for the whole inner scan,
    // that buffer reflects the current left row each time the inner is re-evaluated.
    internal sealed class ExecutableApply : ExecutableOperator
    {
        private static readonly EmittedPredicate AlwaysTrue = _ => true;
        private static readonly EmittedPredicate AlwaysFalse = _ => false;

        private readonly ExecutableOperator _left;
        private readonly ExecutableOperator _right;
        private readonly LogicalApplyKind _applyKind;
        private readonly ValueSlot? _probe;

        public ExecutableApply(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right, LogicalApplyKind applyKind, ValueSlot? probe)
            : base(outputValueSlots)
        {
            _left = left;
            _right = right;
            _applyKind = applyKind;
            _probe = probe;
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var left = _left.CreateIterator(outer);

            // The right side sees the left's columns as outer references (plus whatever
            // outer this apply was itself given, for nested applies).
            var innerOuter = outer is null ? left.RowBuffer : new CombinedRowBuffer(outer, left.RowBuffer);
            var right = _right.CreateIterator(innerOuter);

            switch (_applyKind)
            {
                case LogicalApplyKind.Inner:
                    return new InnerNestedLoopsIterator(left, right, AlwaysTrue, AlwaysFalse);
                case LogicalApplyKind.LeftOuter:
                    return new LeftOuterNestedLoopsIterator(left, right, AlwaysTrue, AlwaysFalse);
                case LogicalApplyKind.LeftSemi:
                    return _probe is null
                        ? new LeftSemiNestedLoopsIterator(left, right, AlwaysTrue, AlwaysFalse)
                        : new ProbingLeftSemiNestedLoopsIterator(left, right, AlwaysTrue);
                case LogicalApplyKind.LeftAntiSemi:
                    return new LeftAntiSemiNestedLoopsIterator(left, right, AlwaysTrue, AlwaysFalse);
                default:
                    throw ExceptionBuilder.UnexpectedValue(_applyKind);
            }
        }
    }
}
