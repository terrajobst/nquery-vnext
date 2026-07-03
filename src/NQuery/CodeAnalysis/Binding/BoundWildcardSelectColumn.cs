using System.Collections.Immutable;

using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundWildcardSelectColumn : BoundNode
{
    private readonly ImmutableArray<TableColumnInstanceSymbol> _tableColumns;

    public BoundWildcardSelectColumn(TableInstanceSymbol? table, IEnumerable<TableColumnInstanceSymbol> columns)
    {
        ThrowIfNull(columns);

        Table = table;
        _tableColumns = [.. columns];
        QueryColumns = [.. _tableColumns.Select(c => new QueryColumnInstanceSymbol(c.Name, c.BoundValue))];
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.WildcardSelectColumn; }
    }

    public TableInstanceSymbol? Table { get; }

    public ImmutableArray<TableColumnInstanceSymbol> TableColumns
    {
        get { return _tableColumns; }
    }

    public ImmutableArray<QueryColumnInstanceSymbol> QueryColumns { get; }
}
