using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Symbols;

internal sealed class DerivedTableSymbol : TableSymbol
{
    internal DerivedTableSymbol(IEnumerable<ColumnSymbol> columns)
        : base(string.Empty)
    {
        ThrowIfNull(columns);

        Columns = [.. columns];
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Table; }
    }

    public override TableKind TableKind
    {
        get { return TableKind.DerivedTable; }
    }

    public override Type Type
    {
        get { return TypeFacts.Missing; }
    }

    public override ImmutableArray<ColumnSymbol> Columns { get; }
}
