using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundComputeRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ImmutableArray<BoundComputedValue> _definedValues;

        public BoundComputeRelation(BoundRelation input, IEnumerable<BoundComputedValue> definedValues)
        {
            _input = input;
            _definedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ComputeRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ImmutableArray<BoundComputedValue> DefinedValues
        {
            get { return _definedValues; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _definedValues.Select(c => c.ValueSlot);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues().Concat(GetDefinedValues());
        }

        public BoundComputeRelation Update(BoundRelation input, IEnumerable<BoundComputedValue> definedValues)
        {
            var newDefinedValues = definedValues.ToImmutableArray();

            if (input == _input && newDefinedValues == _definedValues)
                return this;

            return new BoundComputeRelation(input, newDefinedValues);
        }
    }
}