using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableComputeScalar : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly RowBufferLayout _computedLayout;
    private readonly Action<RowBuffer, ArrayRowBuffer> _writer;

    public ExecutableComputeScalar(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalComputedValue> definedValues, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;

        // Compile the computed expressions once into typed stores. When this compute is
        // correlated (inside an Apply's right), the expressions see the outer slots ahead
        // of the input's, matching the (outer ++ input) buffer fed at run time.
        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = ExpressionCompiler.CreateSlotIndices(slots);

        // The computed columns form their own little buffer (appended to the input's);
        // each defined value writes into its column of that buffer.
        _computedLayout = RowBufferLayout.Create(definedValues.Select(v => v.ValueSlot.Type));
        _writer = ExpressionCompiler.CompileRowWriter(definedValues, _computedLayout, slotIndices);
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        return new ComputeScalarIterator(_input.CreateIterator(workTables, outer), _writer, _computedLayout, outer);
    }
}
