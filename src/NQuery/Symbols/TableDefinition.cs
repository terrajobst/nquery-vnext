using System.Collections;
using System.Collections.Immutable;

using NQuery.Hosting;

namespace NQuery.Symbols
{
    public abstract class TableDefinition
    {
        private ImmutableArray<ColumnDefinition> _columns;

        public ImmutableArray<ColumnDefinition> Columns
        {
            get
            {
                if (_columns.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _columns, GetColumns().ToImmutableArray());

                return _columns;
            }
        }

        public abstract string Name { get; }
        public abstract Type RowType { get; }

        protected abstract IEnumerable<ColumnDefinition> GetColumns();

        public abstract IEnumerable GetRows();

        public static TableDefinition Create<T>(string name, IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);

            return Create(name, source, new ReflectionProvider());
        }

        public static TableDefinition Create<T>(string name, IEnumerable<T> source, IPropertyProvider propertyProvider)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(propertyProvider);

            return Create(name, source, typeof(T), propertyProvider);
        }

        public static TableDefinition Create(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(rowType);
            ArgumentNullException.ThrowIfNull(propertyProvider);

            return new EnumerableTableDefinition(name, source, rowType, propertyProvider);
        }
    }
}