using System.Collections.Immutable;

namespace NQuery.Metadata;

public sealed class RelationshipDefinition
{
    private RelationshipDefinition(TableDefinition parentTable, ImmutableArray<ColumnDefinition> parentColumns, TableDefinition childTable, ImmutableArray<ColumnDefinition> childColumns)
    {
        ParentTable = parentTable;
        ParentColumns = parentColumns;
        ChildTable = childTable;
        ChildColumns = childColumns;
    }

    public static RelationshipDefinition Create(TableDefinition parentTable, IReadOnlyCollection<ColumnDefinition> parentColumns, TableDefinition childTable, IReadOnlyCollection<ColumnDefinition> childColumns)
    {
        ThrowIfNull(parentTable);
        ThrowIfNull(parentColumns);
        ThrowIfNull(childTable);
        ThrowIfNull(childColumns);

        if (parentColumns.Count == 0)
            throw new ArgumentException(Resources.ParentColumnsMustContainAtLeastOneColumn, nameof(parentColumns));

        if (childColumns.Count != parentColumns.Count)
            throw new ArgumentException(Resources.ChildColumnsMustHaveSameSizeAsParentColumns, nameof(childColumns));

        if (parentColumns.Any(c => !parentTable.Columns.Contains(c)))
            throw new ArgumentException(Resources.AllParentColumnsMustBelongToSameTable, nameof(parentColumns));

        if (childColumns.Any(c => !childTable.Columns.Contains(c)))
            throw new ArgumentException(Resources.AllChildColumnsMustBelongToSameTable, nameof(childColumns));

        return new RelationshipDefinition(parentTable, [.. parentColumns], childTable, [.. childColumns]);
    }

    public int ColumnCount
    {
        get { return ParentColumns.Length; }
    }

    public TableDefinition ParentTable { get; }

    public ImmutableArray<ColumnDefinition> ParentColumns { get; }

    public TableDefinition ChildTable { get; }

    public ImmutableArray<ColumnDefinition> ChildColumns { get; }
}
