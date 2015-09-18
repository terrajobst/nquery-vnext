using System;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class SchemaTableSymbol : TableSymbol
    {
        private readonly TableDefinition _tableDefinition;

        public SchemaTableSymbol(TableDefinition tableDefinition)
            : base(GetName(tableDefinition), GetColumns(tableDefinition))
        {
            _tableDefinition = tableDefinition;
        }

        private static string GetName(TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
                throw new ArgumentNullException(nameof(tableDefinition));

            return tableDefinition.Name;
        }

        private static ImmutableArray<SchemaColumnSymbol> GetColumns(TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
                throw new ArgumentNullException(nameof(tableDefinition));

            return tableDefinition.Columns.Select(c => new SchemaColumnSymbol(c)).ToImmutableArray();
        }

        public TableDefinition Definition
        {
            get { return _tableDefinition; }
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public override Type Type
        {
            get { return _tableDefinition.RowType; }
        }
    }
}