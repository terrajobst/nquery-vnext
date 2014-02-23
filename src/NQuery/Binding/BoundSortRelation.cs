using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class BoundSortRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly IList<BoundSortedValue> _sortedValues;

        public BoundSortRelation(BoundRelation input, IList<BoundSortedValue> sortedValues)
        {
            _input = input;
            _sortedValues = sortedValues;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SortRelation;}
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public IList<BoundSortedValue> SortedValues
        {
            get { return _sortedValues; }
        }
    }
}