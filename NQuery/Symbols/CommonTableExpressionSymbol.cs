using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language.Binding;
using NQuery.Language.BoundNodes;

namespace NQuery.Language.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        private readonly BoundQuery _query;
        private readonly ReadOnlyCollection<BoundQuery> _recursiveMembers;

        internal CommonTableExpressionSymbol(string name, IList<ColumnSymbol> columns, BoundQuery query)
            : this(name, columns, query, s => new BoundQuery[0])
        {
        }

        internal CommonTableExpressionSymbol(string name, IList<ColumnSymbol> columns, BoundQuery query, Func<CommonTableExpressionSymbol, IList<BoundQuery>> lazyBoundRecursiveMembers)
            : base(name, columns)
        {
            _query = query;
            _recursiveMembers = new ReadOnlyCollection<BoundQuery>(lazyBoundRecursiveMembers(this));
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.CommonTableExpression; }
        }

        public override Type Type
        {
            get { return KnownTypes.Missing; }
        }

        internal BoundQuery Query
        {
            get { return _query; }
        }

        internal ReadOnlyCollection<BoundQuery> RecursiveMembers
        {
            get { return _recursiveMembers; }
        }
    }
}