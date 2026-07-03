using System.Collections.Immutable;

using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Symbols;

internal sealed class CommonTableExpressionSymbol : TableSymbol
{
    private readonly BoundQuery? _anchor;
    private readonly ImmutableArray<BoundQuery> _recursiveMembers;

    internal CommonTableExpressionSymbol(
        string name,
        Func<CommonTableExpressionSymbol, (BoundQuery? Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder
    )
        : this(name, anchorBinder, _ => [])
    {
    }

    internal CommonTableExpressionSymbol(
        string name,
        Func<CommonTableExpressionSymbol, (BoundQuery? Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder,
        Func<CommonTableExpressionSymbol, ImmutableArray<BoundQuery>> recursiveBinder
    )
        : base(name)
    {
        ThrowIfNull(name);
        ThrowIfNull(anchorBinder);
        ThrowIfNull(recursiveBinder);

        (_anchor, Columns) = anchorBinder(this);
        _recursiveMembers = recursiveBinder(this);
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Table; }
    }

    public override TableKind TableKind
    {
        get { return TableKind.CommonTableExpression; }
    }

    public override Type Type
    {
        get { return TypeFacts.Missing; }
    }

    public override ImmutableArray<ColumnSymbol> Columns { get; }

    internal BoundQuery? Anchor => _anchor;

    internal ImmutableArray<BoundQuery> RecursiveMembers => _recursiveMembers;
}
