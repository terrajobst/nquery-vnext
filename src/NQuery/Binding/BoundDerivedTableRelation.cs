using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundDerivedTableRelation : BoundRelation
    {
        public BoundDerivedTableRelation(TableInstanceSymbol tableInstance, BoundRelation relation)
        {
            TableInstance = tableInstance;
            Relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableRelation; }
        }

        public TableInstanceSymbol TableInstance { get; }

        public BoundRelation Relation { get; }

        public BoundDerivedTableRelation Update(TableInstanceSymbol tableInstance, BoundRelation relation)
        {
            if (tableInstance == TableInstance && Relation == relation)
                return this;

            return new BoundDerivedTableRelation(tableInstance, relation);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Relation.GetOutputValues();
        }
    }
}