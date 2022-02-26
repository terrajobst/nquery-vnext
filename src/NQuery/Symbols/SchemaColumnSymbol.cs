namespace NQuery.Symbols
{
    public sealed class SchemaColumnSymbol : ColumnSymbol
    {
        public SchemaColumnSymbol(ColumnDefinition columnDefinition)
            : base(columnDefinition.Name, columnDefinition.DataType)
        {
            Definition = columnDefinition;
        }

        public ColumnDefinition Definition { get; }
    }
}