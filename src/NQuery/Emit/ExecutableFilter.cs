using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

internal sealed class ExecutableFilter : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly EmittedPredicate _predicate;

    public ExecutableFilter(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalExpression> conditions, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;

        // Compile the conjuncts once. When this filter is correlated (inside an
        // Apply's right), the predicate sees the outer slots ahead of the input's,
        // matching the (outer ++ input) buffer fed at run time.
        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(slots);
        var predicates = conditions
                         .Select(c => EmittedExpressionCompiler.CompilePredicate(c, slotIndices))
                         .ToImmutableArray();
        _predicate = Conjoin(predicates);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        return new EmittedFilterIterator(_input.CreateIterator(outer), _predicate, outer);
    }

    // Each conjunct already yields false on NULL, so AND-ing gives WHERE semantics.
    private static EmittedPredicate Conjoin(ImmutableArray<EmittedPredicate> predicates)
    {
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
