using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundComputeRelation : BoundRelation
    {
        public BoundComputeRelation(BoundRelation input, IEnumerable<BoundComputedValue> definedValues)
        {
            Input = input;
            DefinedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ComputeRelation; }
        }

        public BoundRelation Input { get; }

        public ImmutableArray<BoundComputedValue> DefinedValues { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Input.GetDefinedValues().Concat(DefinedValues.Select(c => c.ValueSlot));
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Input.GetOutputValues().Concat(DefinedValues.Select(c => c.ValueSlot));
        }

        public BoundComputeRelation Update(BoundRelation input, IEnumerable<BoundComputedValue> definedValues)
        {
            var newDefinedValues = definedValues.ToImmutableArray();

            if (input == Input && newDefinedValues == DefinedValues)
                return this;

            return new BoundComputeRelation(input, newDefinedValues);
        }
    }
}