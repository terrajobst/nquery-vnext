using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
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

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return new[] { _tableInstance };
        }
    }
}