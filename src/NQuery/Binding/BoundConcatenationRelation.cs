using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundConcatenationRelation : BoundRelation
    {
        private readonly ImmutableArray<BoundRelation> _inputs;
        private readonly ImmutableArray<BoundUnifiedValue> _definedValues;

        public BoundConcatenationRelation(IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            _inputs = inputs.ToImmutableArray();
            _definedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConcatenationRelation; }
        }

        public ImmutableArray<BoundRelation> Inputs
        {
            get { return _inputs; }
        }

        public ImmutableArray<BoundUnifiedValue> DefinedValues
        {
            get { return _definedValues; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _definedValues.Select(v => v.ValueSlot);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return GetDefinedValues();
        }

        public BoundConcatenationRelation Update(IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            var newInputs = inputs.ToImmutableArray();
            var newDefinedValues = definedValues.ToImmutableArray();

            if (newInputs == _inputs && newDefinedValues == _definedValues)
                return this;

            return new BoundConcatenationRelation(newInputs, newDefinedValues);
        }
    }
}