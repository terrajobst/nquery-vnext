using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        internal CommonTableExpressionSymbol(string name, IEnumerable<ColumnSymbol> columns, BoundQuery query)
            : this(name, columns, query, s => ImmutableArray<BoundQuery>.Empty)
        {
        }

        internal CommonTableExpressionSymbol(string name, IEnumerable<ColumnSymbol> columns, BoundQuery query, Func<CommonTableExpressionSymbol, ImmutableArray<BoundQuery>> lazyBoundRecursiveMembers)
            : base(name, columns)
        {
            Query = query;
            RecursiveMembers = lazyBoundRecursiveMembers(this);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.CommonTableExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        internal BoundQuery Query { get; }

        internal ImmutableArray<BoundQuery> RecursiveMembers { get; }
    }
}