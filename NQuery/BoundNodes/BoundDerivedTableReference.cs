using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundDerivedTableReference : BoundTableReference
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly BoundQuery _query;

        public BoundDerivedTableReference(TableInstanceSymbol tableInstance, BoundQuery query)
        {
            _tableInstance = tableInstance;
            _query = query;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableReference; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundQuery Query
        {
            get { return _query; }
        }

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return new[] { _tableInstance };
        }
    }
}