using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public sealed class SchemaTableSymbol : TableSymbol
    {
        public SchemaTableSymbol(TableDefinition tableDefinition)
            : base(GetName(tableDefinition))
        {
            Definition = tableDefinition;
            Columns = tableDefinition.Columns.Select(c => (ColumnSymbol)new SchemaColumnSymbol(c)).ToImmutableArray();
        }

        private static string GetName(TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
                throw new ArgumentNullException(nameof(tableDefinition));

            return tableDefinition.Name;
        }

        public TableDefinition Definition { get; }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public override Type Type
        {
            get { return Definition.RowType; }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }
    }
}