using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _tableInstance;

        public BoundTableRelation(TableInstanceSymbol tableInstance)
        {
            _tableInstance = tableInstance;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableRelation; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundTableRelation Update(TableInstanceSymbol tableInstance)
        {
            if (tableInstance == _tableInstance)
                return this;

            return new BoundTableRelation(tableInstance);
        }
    }
}