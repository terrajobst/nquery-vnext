using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundUnionRelation : BoundRelation
    {
        public BoundUnionRelation(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            IsUnionAll = isUnionAll;
            Inputs = inputs.ToImmutableArray();
            DefinedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnionRelation; }
        }

        public bool IsUnionAll { get; }

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

        public BoundUnionRelation Update(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            var newInputs = inputs.ToImmutableArray();
            var newDefinedValues = definedValues.ToImmutableArray();

            if (isUnionAll == IsUnionAll && newInputs == Inputs && newDefinedValues == DefinedValues)
                return this;

            return new BoundUnionRelation(isUnionAll, newInputs, newDefinedValues);
        }
    }
}