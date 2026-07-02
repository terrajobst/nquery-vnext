using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Metadata;

public abstract class TableDefinition
{
    private protected TableDefinition()
    {
    }

    private ImmutableArray<ColumnDefinition> _columns;

    public ImmutableArray<ColumnDefinition> Columns
    {
        get
        {
            if (_columns.IsDefault)
                ImmutableInterlocked.InterlockedInitialize(ref _columns, [.. GetColumns()]);

            return _columns;
        }
    }

    public abstract string Name { get; }
    public abstract Type RowType { get; }

    protected abstract IEnumerable<ColumnDefinition> GetColumns();

    public abstract IEnumerable GetRows();

    public static TableDefinition Create<T>(string name, IEnumerable<T> source)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);

        return Create(name, source, new ReflectionProvider());
    }

    public static TableDefinition Create<T>(string name, IEnumerable<T> source, IPropertyProvider propertyProvider)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);
        ThrowIfNull(propertyProvider);

        return Create(name, source, typeof(T), propertyProvider);
    }

    public static TableDefinition Create(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);
        ThrowIfNull(rowType);
        ThrowIfNull(propertyProvider);

        var columns = propertyProvider.GetProperties(rowType)
                                      .Select(p => (ColumnDefinition)new PropertyColumnDefinition(rowType, p))
                                      .ToImmutableArray();
        return new EnumerableTableDefinition(name, source, rowType, columns);
    }

    public static TableDefinition Create<T>(string name, IEnumerable<T> source, params IEnumerable<ColumnDefinition> columns)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);
        ThrowIfNull(columns);

        return Create(name, source, typeof(T), columns);
    }

    public static TableDefinition Create(string name, IEnumerable source, Type rowType, params IEnumerable<ColumnDefinition> columns)
    {
        ThrowIfNull(name);
        ThrowIfNull(source);
        ThrowIfNull(rowType);
        ThrowIfNull(columns);

        return new EnumerableTableDefinition(name, source, rowType, [.. columns]);
    }
}
