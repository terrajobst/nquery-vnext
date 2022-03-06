namespace NQuery.Symbols
{
    public sealed class SchemaColumnSymbol : ColumnSymbol
    {
        public SchemaColumnSymbol(ColumnDefinition columnDefinition)
            : base(columnDefinition.Name)
        {
            Definition = columnDefinition;
        }

        public override Type Type => Definition.DataType;

        public ColumnDefinition Definition { get; }
    }
}