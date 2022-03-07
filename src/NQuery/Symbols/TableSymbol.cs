using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class TableSymbol : Symbol
    {
        private protected TableSymbol(string name)
            : base(name)
        {
        }

        public abstract ImmutableArray<ColumnSymbol> Columns { get; }
    }
}