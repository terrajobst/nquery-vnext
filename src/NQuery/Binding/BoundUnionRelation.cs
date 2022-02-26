using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundUnionRelation : BoundRelation
    {
        public BoundUnionRelation(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues, IEnumerable<IComparer> comparers)
        {
            IsUnionAll = isUnionAll;
            Inputs = inputs.ToImmutableArray();
            DefinedValues = definedValues.ToImmutableArray();
            Comparers = comparers.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnionRelation; }
        }

        public bool IsUnionAll { get; }

        public ImmutableArray<BoundRelation> Inputs { get; }

        public ImmutableArray<BoundUnifiedValue> DefinedValues { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return DefinedValues.Select(v => v.ValueSlot);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return GetDefinedValues();
        }

        public BoundUnionRelation Update(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues, IEnumerable<IComparer> comparers)
        {
            var newInputs = inputs.ToImmutableArray();
            var newDefinedValues = definedValues.ToImmutableArray();
            var newComparers = comparers.ToImmutableArray();

            if (isUnionAll == IsUnionAll && newInputs == Inputs && newDefinedValues == DefinedValues && newComparers == Comparers)
                return this;

            return new BoundUnionRelation(isUnionAll, newInputs, newDefinedValues, newComparers);
        }
    }
}