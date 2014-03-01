using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundComputeRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ReadOnlyCollection<BoundComputedValue> _definedValues;

        public BoundComputeRelation(BoundRelation input, IList<BoundComputedValue> definedValues)
        {
            _input = input;
            _definedValues = new ReadOnlyCollection<BoundComputedValue>(definedValues);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ComputeRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<BoundComputedValue> DefinedValues
        {
            get { return _definedValues; }
        }

        public BoundComputeRelation Update(BoundRelation input, IList<BoundComputedValue> definedValues)
        {
            if (input == _input && definedValues == _definedValues)
                return this;

            return new BoundComputeRelation(input, definedValues);
        }
    }
}