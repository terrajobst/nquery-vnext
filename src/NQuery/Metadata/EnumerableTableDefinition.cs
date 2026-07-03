using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Metadata;

internal sealed class EnumerableTableDefinition : TableDefinition
{
    private readonly IEnumerable _source;
    private readonly Type _rowType;
    private readonly ImmutableArray<ColumnDefinition> _columns;

    public EnumerableTableDefinition(string name, IEnumerable source, Type rowType, ImmutableArray<ColumnDefinition> columns)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);
        ThrowIfNull(rowType);

        Name = name;
        _source = source;
        _rowType = rowType;
        _columns = columns;
    }

    protected override IEnumerable<ColumnDefinition> GetColumns()
    {
        return _columns;
    }

    public override IEnumerable GetRows()
    {
        return _source;
    }

    public override string Name { get; }

    public override Type RowType
    {
        get { return _rowType; }
    }
}
