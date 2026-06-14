using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

// Passes its input through, raising an error on a row that fails the condition.
// The condition is compiled once, against the (outer ++ input) layout when the
// assert is correlated (inside an Apply's right), matching the buffer the iterator
// feeds at run time.
internal sealed class ExecutableAssert : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly EmittedPredicate _predicate;
    private readonly string _message;

    public ExecutableAssert(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, LogicalExpression condition, string message, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;
        _message = message;

        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(slots);

        // NOTE: CompilePredicate coalesces a NULL result to false, so a NULL condition
        //       fires the assert. That suits the only current use (count <= 1, never
        //       NULL); a nullable assert condition would need NULL-passes semantics
        //       (the assert treated NULL as success).
        _predicate = EmittedExpressionCompiler.CompilePredicate(condition, slotIndices);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        return new EmittedAssertIterator(_input.CreateIterator(outer), _predicate, _message, outer);
    }
}
