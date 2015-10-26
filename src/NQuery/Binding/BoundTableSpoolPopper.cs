using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundTableSpoolPopper : BoundRelation
    {
        private readonly ImmutableArray<ValueSlot> _outputs;

        public BoundTableSpoolPopper(ImmutableArray<ValueSlot> outputs)
        {
            _outputs = outputs;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableSpoolPopper; }
        }

        public ImmutableArray<ValueSlot> Outputs
        {
            get { return _outputs; }
        }

        public BoundTableSpoolPopper Update(IEnumerable<ValueSlot> outputs)
        {
            var newOutputs = outputs.ToImmutableArray();
            if (newOutputs == _outputs)
                return this;

            return new BoundTableSpoolPopper(_outputs);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _outputs;
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _outputs;
        }
    }
}