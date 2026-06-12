#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Refactor.Iterators;

namespace NQuery.Refactor.Emit
{
    // Concatenation (UNION ALL): each input contributes its rows in turn. The defined
    // values describe, per input, which of that input's slots feed each unified output
    // column, so every input is projected into the same output order. A plain UNION's
    // distinctness is applied by a sort the planner places above this node.
    internal sealed class ExecutableConcatenation : ExecutableOperator
    {
        private readonly ImmutableArray<ExecutableOperator> _inputs;
        private readonly ImmutableArray<BoundUnifiedValue> _definedValues;

        public ExecutableConcatenation(ImmutableArray<ValueSlot> outputValueSlots, ImmutableArray<ExecutableOperator> inputs, ImmutableArray<BoundUnifiedValue> definedValues)
            : base(outputValueSlots)
        {
            _inputs = inputs;
            _definedValues = definedValues;
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var inputs = _inputs.Select(i => i.CreateIterator(outer)).ToImmutableArray();
            var inputEntries = Enumerable.Range(0, inputs.Length)
                                         .Select(i =>
                                         {
                                             var allocation = Allocate(_inputs[i], inputs[i]);
                                             return _definedValues.Select(d => allocation[d.InputValueSlots[i]]).ToImmutableArray();
                                         });
            return new EmittedConcatenationIterator(inputs, inputEntries);
        }
    }
}
