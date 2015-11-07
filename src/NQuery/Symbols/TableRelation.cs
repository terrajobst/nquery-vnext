using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class TableRelation
    {
        public TableRelation(TableSymbol parentTable, IReadOnlyCollection<ColumnSymbol> parentColumns, TableSymbol childTable, IReadOnlyCollection<ColumnSymbol> childColumns)
        {
            if (parentColumns == null)
                throw new ArgumentNullException(nameof(parentColumns));

            if (childColumns == null)
                throw new ArgumentNullException(nameof(childColumns));

            if (parentColumns.Count == 0)
                throw new ArgumentException(Resources.ParentColumnsMustContainAtLeastOneColumn, nameof(parentColumns));

            if (childColumns.Count != parentColumns.Count)
                throw new ArgumentException(Resources.ChildColumnsMustHaveSameSizeAsParentColumns, nameof(childColumns));

            if (parentColumns.Any(c => !parentTable.Columns.Contains(c)))
                throw new ArgumentException(Resources.AllParentColumnsMustBelongToSameTable, nameof(parentColumns));

            if (childColumns.Any(c => !childTable.Columns.Contains(c)))
                throw new ArgumentException(Resources.AllChildColumnsMustBelongToSameTable, nameof(childColumns));

            ParentTable = parentTable;
            ParentColumns = parentColumns.ToImmutableArray();
            ChildTable = childTable;
            ChildColumns = childColumns.ToImmutableArray();
        }

        public int ColumnCount
        {
            get { return ParentColumns.Length; }
        }

        public TableSymbol ParentTable { get; }

        public ImmutableArray<ColumnSymbol> ParentColumns { get; }

        public TableSymbol ChildTable { get; }

        public ImmutableArray<ColumnSymbol> ChildColumns { get; }
    }
}