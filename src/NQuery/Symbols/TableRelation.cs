using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class TableRelation
    {
        private readonly TableSymbol _parentTable;
        private readonly ImmutableArray<ColumnSymbol> _parentColumns;
        private readonly TableSymbol _childTable;
        private readonly ImmutableArray<ColumnSymbol> _childColumns;

        public TableRelation(TableSymbol parentTable, IReadOnlyCollection<ColumnSymbol> parentColumns, TableSymbol childTable, IReadOnlyCollection<ColumnSymbol> childColumns)
        {
            if (parentColumns == null)
                throw new ArgumentNullException("parentColumns");

            if (childColumns == null)
                throw new ArgumentNullException("childColumns");

            if (parentColumns.Count == 0)
                throw new ArgumentException(Resources.ParentColumnsMustContainAtLeastOneColumn, "parentColumns");

            if (childColumns.Count != parentColumns.Count)
                throw new ArgumentException(Resources.ChildColumnsMustHaveSameSizeAsParentColumns, "childColumns");

            if (parentColumns.Any(c => !parentTable.Columns.Contains(c)))
                throw new ArgumentException(Resources.AllParentColumnsMustBelongToSameTable, "parentColumns");

            if (childColumns.Any(c => !childTable.Columns.Contains(c)))
                throw new ArgumentException(Resources.AllChildColumnsMustBelongToSameTable, "childColumns");

            _parentTable = parentTable;
            _parentColumns = parentColumns.ToImmutableArray();
            _childTable = childTable;
            _childColumns = childColumns.ToImmutableArray();
        }

        public int ColumnCount
        {
            get { return _parentColumns.Length; }
        }

        public TableSymbol ParentTable
        {
            get { return _parentTable; }
        }

        public ImmutableArray<ColumnSymbol> ParentColumns
        {
            get { return _parentColumns; }
        }

        public TableSymbol ChildTable
        {
            get { return _childTable; }
        }

        public ImmutableArray<ColumnSymbol> ChildColumns
        {
            get { return _childColumns; }
        }
    }
}