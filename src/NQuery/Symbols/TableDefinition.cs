#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
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
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Create(name, source, new ReflectionProvider());
        }

        public static TableDefinition Create<T>(string name, IEnumerable<T> source, IPropertyProvider propertyProvider)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (propertyProvider == null)
                throw new ArgumentNullException(nameof(propertyProvider));

            return Create(name, source, typeof(T), propertyProvider);
        }

        public static TableDefinition Create(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (rowType == null)
                throw new ArgumentNullException(nameof(rowType));

            if (propertyProvider == null)
                throw new ArgumentNullException(nameof(propertyProvider));

            return new EumerableTableDefinition(name, source, rowType, propertyProvider);
        }
    }
}