using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundUnionRelation : BoundRelation
    {
        private readonly bool _isUnionAll;
        private readonly ImmutableArray<BoundRelation> _inputs;
        private readonly ImmutableArray<BoundUnifiedValue> _definedValues;

        public BoundUnionRelation(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            _isUnionAll = isUnionAll;
            _inputs = inputs.ToImmutableArray();
            _definedValues = definedValues.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnionRelation; }
        }

        public bool IsUnionAll
        {
            get { return _isUnionAll; }
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

        public BoundUnionRelation Update(bool isUnionAll, IEnumerable<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues)
        {
            var newInputs = inputs.ToImmutableArray();
            var newDefinedValues = definedValues.ToImmutableArray();

            if (isUnionAll == _isUnionAll && newInputs == _inputs && newDefinedValues == _definedValues)
                return this;

            return new BoundUnionRelation(isUnionAll, newInputs, newDefinedValues);
        }
    }
}