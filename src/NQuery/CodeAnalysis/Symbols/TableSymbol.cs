using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Symbols;

public abstract class TableSymbol : Symbol
{
    private protected TableSymbol(string name)
        : base(name)
    {
    }

    public abstract TableKind TableKind { get; }

    public virtual TableDefinition? Definition
    {
        get { return null; }
    }

    public abstract ImmutableArray<ColumnSymbol> Columns { get; }
}
