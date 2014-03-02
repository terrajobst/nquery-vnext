using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundProjectRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _outputs;

        public BoundProjectRelation(BoundRelation input, IList<ValueSlot> outputs)
        {
            _input = input;
            _outputs = new ReadOnlyCollection<ValueSlot>(outputs);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ProjectRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<ValueSlot> Outputs
        {
            get { return _outputs; }
        }

        public BoundProjectRelation Update(BoundRelation input, IList<ValueSlot> outputs)
        {
            if (input == _input && outputs == _outputs)
                return this;

            return new BoundProjectRelation(input, outputs);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _outputs;
        }
    }
}