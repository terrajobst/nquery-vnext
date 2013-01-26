using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Algebra
{
    internal sealed class AlgebraComputeNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<AlgebraComputedValue> _definedValues;

        public AlgebraComputeNode(AlgebraRelation input, IList<AlgebraComputedValue> definedValues)
        {
            _input = input;
            _definedValues = new ReadOnlyCollection<AlgebraComputedValue>(definedValues);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Compute; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<AlgebraComputedValue> DefinedValues
        {
            get { return _definedValues; }
        }
    }
}