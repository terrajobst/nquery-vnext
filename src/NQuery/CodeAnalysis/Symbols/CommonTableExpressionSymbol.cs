using System.Collections.Immutable;

using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Symbols;

internal sealed class CommonTableExpressionSymbol : TableSymbol
{
    private BoundQuery? _anchor;
    private ImmutableArray<ColumnSymbol> _columns = [];
    private ImmutableArray<BoundQuery> _recursiveMembers = [];

    internal CommonTableExpressionSymbol(string name)
        : base(name)
    {
        ThrowIfNull(name);
    }

    // A CTE symbol is bound in place: it must be in scope while its own body is
    // bound so that recursive members can resolve references back to it. The
    // binder therefore constructs the symbol first, then completes it in two
    // phases:
    //
    //   1. BindAnchor sets the anchor and columns. Columns must be available
    //      before recursive members bind, since a recursive reference resolves
    //      its columns through this symbol.
    //   2. BindRecursiveMembers sets the recursive members (empty for a
    //      non-recursive CTE).
    //
    // A malformed CTE skips both phases, which is the only way Anchor stays null.

    internal void BindAnchor(BoundQuery anchor, ImmutableArray<ColumnSymbol> columns)
    {
        _anchor = anchor;
        _columns = columns;
    }

    internal void BindRecursiveMembers(ImmutableArray<BoundQuery> recursiveMembers)
    {
        _recursiveMembers = recursiveMembers;
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

    public override ImmutableArray<ColumnSymbol> Columns => _columns;

    internal BoundQuery? Anchor => _anchor;

    internal ImmutableArray<BoundQuery> RecursiveMembers => _recursiveMembers;
}
