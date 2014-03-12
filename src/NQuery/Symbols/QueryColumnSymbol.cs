using System;

namespace NQuery.Symbols
{
    public sealed class QueryColumnSymbol : ColumnSymbol
    {
        private readonly QueryColumnInstanceSymbol _queryColumnInstance;

        public QueryColumnSymbol(QueryColumnInstanceSymbol queryColumnInstance)
            : base(queryColumnInstance.Name, queryColumnInstance.Type)
        {
            _queryColumnInstance = queryColumnInstance;
        }

        public QueryColumnInstanceSymbol QueryColumnInstance
        {
            get { return _queryColumnInstance; }
        }
    }
}