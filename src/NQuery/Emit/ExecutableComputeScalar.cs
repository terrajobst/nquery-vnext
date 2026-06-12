#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.AlgebraBinding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableComputeScalar : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly ImmutableArray<EmittedFunction> _functions;

        public ExecutableComputeScalar(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalComputedValue> definedValues, ImmutableArray<ValueSlot> outerSlots)
            : base(outputValueSlots)
        {
            _input = input;

            // Compile the computed expressions once. When this compute is correlated
            // (inside an Apply's right), the expressions see the outer slots ahead of
            // the input's, matching the (outer ++ input) buffer fed at run time.
            var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(slots);
            _functions = definedValues
                         .Select(v => EmittedExpressionCompiler.CompileFunction(v.Expression, slotIndices))
                         .ToImmutableArray();
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            return new EmittedComputeScalarIterator(_input.CreateIterator(outer), _functions, outer);
        }
    }
}
