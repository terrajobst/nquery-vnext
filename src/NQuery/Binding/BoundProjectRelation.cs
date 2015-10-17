using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundProjectRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ImmutableArray<ValueSlot> _outputs;

        public BoundProjectRelation(BoundRelation input, IEnumerable<ValueSlot> outputs)
        {
            _input = input;
            _outputs = outputs.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ProjectRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ImmutableArray<ValueSlot> Outputs
        {
            get { return _outputs; }
        }

        public BoundProjectRelation Update(BoundRelation input, IEnumerable<ValueSlot> outputs)
        {
            var newOutputs = outputs.ToImmutableArray();

            if (input == _input && newOutputs == _outputs)
                return this;

            return new BoundProjectRelation(input, newOutputs);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _outputs;
        }
    }
}