#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableProject : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly ImmutableArray<ValueSlot> _outputs;

        public ExecutableProject(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<ValueSlot> outputs)
            : base(outputValueSlots)
        {
            _input = input;
            _outputs = outputs;
        }

        public override Iterator CreateIterator()
        {
            var input = _input.CreateIterator();
            var allocation = Allocate(_input, input);
            var entries = _outputs.Select(s => allocation[s]);
            return new ProjectionIterator(input, entries);
        }
    }
}
