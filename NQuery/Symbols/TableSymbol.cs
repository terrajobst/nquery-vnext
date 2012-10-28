using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.Symbols
{
    public abstract class TableSymbol : Symbol
    {
        private readonly ReadOnlyCollection<ColumnSymbol> _columns;

        protected TableSymbol(string name, IList<ColumnSymbol> columns)
            : base(name)
        {
            _columns = new ReadOnlyCollection<ColumnSymbol>(columns);
        }

        public ReadOnlyCollection<ColumnSymbol> Columns
        {
            get { return _columns; }
        }
    }
}