using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Metadata;

public abstract class TableDefinition
{
    private protected TableDefinition(string name, Type rowType, ImmutableArray<ColumnDefinition> columns)
    {
        ThrowIfNull(name);
        ThrowIfNull(rowType);
        ThrowIfNull(columns);

        Name = name;
        RowType = rowType;
        Columns = columns;
    }

    public string Name { get; }

    public Type RowType { get; }

    public ImmutableArray<ColumnDefinition> Columns { get; }

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
                                      .Select(p => ColumnDefinition.Create(rowType, p))
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

    private sealed class EnumerableTableDefinition : TableDefinition
    {
        private readonly IEnumerable _source;

        public EnumerableTableDefinition(string name, IEnumerable source, Type rowType, ImmutableArray<ColumnDefinition> columns)
            : base(name, rowType, columns)
        {
            _source = source;
        }

        public override IEnumerable GetRows()
        {
            return _source;
        }
    }
}
