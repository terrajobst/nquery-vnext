using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundConcatenationRelation : BoundRelation
    {
        public BoundConcatenationRelation(IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            Inputs = inputs.ToImmutableArray();
            DefinedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConcatenationRelation; }
        }

        public ImmutableArray<BoundRelation> Inputs { get; }

        public ImmutableArray<BoundUnifiedValue> DefinedValues { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return DefinedValues.Select(v => v.ValueSlot);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return GetDefinedValues();
        }

        public BoundConcatenationRelation Update(IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            var newInputs = inputs.ToImmutableArray();
            var newDefinedValues = definedValues.ToImmutableArray();

            if (newInputs == Inputs && newDefinedValues == DefinedValues)
                return this;

            return new BoundConcatenationRelation(newInputs, newDefinedValues);
        }
    }
}