using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableFilter : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly CompiledPredicate _predicate;

    public ExecutableFilter(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalExpression> conditions, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;

        // Compile the conjuncts once. When this filter is correlated (inside an
        // Apply's right), the predicate sees the outer slots ahead of the input's,
        // matching the (outer ++ input) buffer fed at run time.
        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = ExpressionCompiler.CreateSlotIndices(slots);
        var predicates = conditions
                         .Select(c => ExpressionCompiler.CompilePredicate(c, slotIndices))
                         .ToImmutableArray();
        _predicate = Conjoin(predicates);
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        return new FilterIterator(_input.CreateIterator(workTables, outer), _predicate, outer);
    }

    // Each conjunct already yields false on NULL, so AND-ing gives WHERE semantics.
    private static CompiledPredicate Conjoin(ImmutableArray<CompiledPredicate> predicates)
    {
        if (predicates is [var predicate])
            return predicate;

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
