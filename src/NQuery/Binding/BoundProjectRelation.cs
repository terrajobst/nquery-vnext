using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundProjectRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly IReadOnlyCollection<ValueSlot> _outputs;

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

        public IReadOnlyCollection<ValueSlot> Outputs
        {
            get { return _outputs; }
        }
    }
}