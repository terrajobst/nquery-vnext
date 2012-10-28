using System;
using System.Collections.Generic;

namespace NQuery.Language.Symbols
{
    public sealed class SchemaTableSymbol : TableSymbol
    {
        private readonly Type _rowType;

        public SchemaTableSymbol(string name, IList<ColumnSymbol> columns, Type rowType)
            : base(name, columns)
        {
            _rowType = rowType;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public override Type Type
        {
            get { return _rowType; }
        }
    }
}