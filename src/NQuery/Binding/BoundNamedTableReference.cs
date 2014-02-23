using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundNamedTableReference : BoundTableReference
    {
        private readonly TableInstanceSymbol _tableInstance;

        public BoundNamedTableReference(TableInstanceSymbol tableInstance)
        {
            _tableInstance = tableInstance;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.NamedTableReference; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }
    }
}