using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundDerivedTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly BoundRelation _relation;

        public BoundDerivedTableRelation(TableInstanceSymbol tableInstance, BoundRelation relation)
        {
            _tableInstance = tableInstance;
            _relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableRelation; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }

        public BoundDerivedTableRelation Update(TableInstanceSymbol tableInstance, BoundRelation relation)
        {
            if (tableInstance == _tableInstance && _relation == relation)
                return this;

            return new BoundDerivedTableRelation(tableInstance, relation);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _relation.GetOutputValues();
        }
    }
}