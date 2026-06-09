#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableComputeScalar : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly ImmutableArray<EmittedFunction> _functions;

        public ExecutableComputeScalar(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalComputedValue> definedValues)
            : base(outputValueSlots)
        {
            _input = input;

            // Compile the computed expressions once, against the input's slot layout.
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(input.OutputValueSlots);
            _functions = definedValues
                         .Select(v => EmittedExpressionCompiler.CompileFunction(v.Expression, slotIndices))
                         .ToImmutableArray();
        }

        public override Iterator CreateIterator()
        {
            return new EmittedComputeScalarIterator(_input.CreateIterator(), _functions);
        }
    }
}
