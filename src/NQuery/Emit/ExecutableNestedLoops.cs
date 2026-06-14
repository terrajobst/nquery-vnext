using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;
using NQuery.Planning;

namespace NQuery.Emit;

// A nested-loops join. The condition and passthru predicate are compiled once,
// against the combined (left ++ right) slot layout, and take the row buffer as a
// parameter -- so the executable join is reusable across CreateIterator() calls.
//
// It is also the physical form of an Apply (a dependent join): outerReferences are
// the left columns the right reads, and the join predicate is simply absent (empty
// conditions => constant true) because the correlation lives inside the right
// subtree's own filters/computes.
internal sealed class ExecutableNestedLoops : ExecutableOperator
{
    private static readonly EmittedPredicate AlwaysTrue = _ => true;
    private static readonly EmittedPredicate AlwaysFalse = _ => false;

    private readonly ExecutableOperator _left;
    private readonly ExecutableOperator _right;
    private readonly PhysicalJoinKind _joinKind;
    private readonly ValueSlot? _probe;
    private readonly ImmutableArray<int> _outerReferenceIndices;
    private readonly EmittedPredicate _predicate;
    private readonly EmittedPredicate _passthruPredicate;

    public ExecutableNestedLoops(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right, PhysicalJoinKind joinKind, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate, ImmutableArray<ValueSlot> outerReferences, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _left = left;
        _right = right;
        _joinKind = joinKind;
        _probe = probe;

        // The outer references are exposed to the right by projecting the left's
        // row buffer down to just these columns; precompute their positions in the
        // left's output (which is the left iterator's row-buffer layout).
        _outerReferenceIndices = outerReferences.Select(s => left.OutputValueSlots.IndexOf(s)).ToImmutableArray();

        // The predicates see both sides, and -- when this join is inside an Apply -- the
        // ambient outer scope ahead of them, so a join condition can reference an outer slot
        // (correlation on the join itself, just like a correlated Filter). Compile against
        // (outer ++ left ++ right), which is exactly the order the iterators feed at run time
        // (the outer buffer prepended to the CombinedRowBuffer). outerSlots is empty iff no
        // Apply encloses this join, in which case the layout is just (left ++ right).
        var combined = left.OutputValueSlots.AddRange(right.OutputValueSlots);
        var predicateSlots = outerSlots.IsEmpty ? combined : outerSlots.AddRange(combined);
        var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(predicateSlots);
        _predicate = CompileConjunction(conditions, slotIndices);
        _passthruPredicate = passthruPredicate is null
            ? AlwaysFalse
            : EmittedExpressionCompiler.CompilePredicate(passthruPredicate, slotIndices);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        var left = _left.CreateIterator(outer);

        // A dependent join exposes its outer references (a projection of the left
        // row) to the right, accumulated onto any outer this node itself sits
        // under. A plain join's right is independent, so it just sees the outer it
        // was handed.
        var rightOuter = outer;
        if (!_outerReferenceIndices.IsEmpty)
        {
            var projectedLeft = new ProjectedRowBuffer(_outerReferenceIndices.Select(i => new RowBufferEntry(left.RowBuffer, i)));
            rightOuter = outer is null ? projectedLeft : new CombinedRowBuffer(outer, projectedLeft);
        }

        var right = _right.CreateIterator(rightOuter);

        switch (_joinKind)
        {
            case PhysicalJoinKind.Inner:
                return new InnerNestedLoopsIterator(left, right, _predicate, _passthruPredicate, outer);
            case PhysicalJoinKind.LeftOuter:
                return new LeftOuterNestedLoopsIterator(left, right, _predicate, _passthruPredicate, outer);
            case PhysicalJoinKind.LeftSemi:
                return _probe is null
                    ? new LeftSemiNestedLoopsIterator(left, right, _predicate, _passthruPredicate, outer)
                    : new ProbingLeftSemiNestedLoopsIterator(left, right, _predicate, outer);
            case PhysicalJoinKind.LeftAntiSemi:
                return new LeftAntiSemiNestedLoopsIterator(left, right, _predicate, _passthruPredicate, outer);
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
