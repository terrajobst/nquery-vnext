using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundProjectRelation : BoundRelation
    {
        public BoundProjectRelation(BoundRelation input, IEnumerable<ValueSlot> outputs)
        {
            Input = input;
            Outputs = outputs.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ProjectRelation; }
        }

        public BoundRelation Input { get; }

        public ImmutableArray<ValueSlot> Outputs { get; }

        public BoundProjectRelation Update(BoundRelation input, IEnumerable<ValueSlot> outputs)
        {
            var newOutputs = outputs.ToImmutableArray();

            if (input == Input && newOutputs == Outputs)
                return this;

            return new BoundProjectRelation(input, newOutputs);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Outputs;
        }
    }
}