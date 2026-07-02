using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Symbols;

internal sealed class SchemaTableSymbol : TableSymbol
{
    internal SchemaTableSymbol(TableDefinition tableDefinition)
        : base(GetName(tableDefinition))
    {
        Definition = tableDefinition;
        Columns = [.. tableDefinition.Columns.Select(c => new ColumnSymbol(c))];
    }

    private static string GetName(TableDefinition tableDefinition)
    {
        ThrowIfNull(tableDefinition);

        return tableDefinition.Name;
    }

    public override TableDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Table; }
    }

    public override TableKind TableKind
    {
        get { return TableKind.SchemaTable; }
    }

    public override Type Type
    {
        get { return Definition.RowType; }
    }

    public override ImmutableArray<ColumnSymbol> Columns { get; }
}
