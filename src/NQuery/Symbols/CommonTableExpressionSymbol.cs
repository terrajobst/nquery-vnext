using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        private readonly BoundQueryRelation _query;
        private readonly ReadOnlyCollection<BoundQueryRelation> _recursiveMembers;

        internal CommonTableExpressionSymbol(string name, IList<ColumnSymbol> columns, BoundQueryRelation query)
            : this(name, columns, query, s => new BoundQueryRelation[0])
        {
        }

        internal CommonTableExpressionSymbol(string name, IList<ColumnSymbol> columns, BoundQueryRelation query, Func<CommonTableExpressionSymbol, IList<BoundQueryRelation>> lazyBoundRecursiveMembers)
            : base(name, columns)
        {
            _query = query;
            _recursiveMembers = new ReadOnlyCollection<BoundQueryRelation>(lazyBoundRecursiveMembers(this));
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.CommonTableExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        internal BoundQueryRelation Query
        {
            get { return _query; }
        }

        internal ReadOnlyCollection<BoundQueryRelation> RecursiveMembers
        {
            get { return _recursiveMembers; }
        }
    }
}