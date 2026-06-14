using System.Collections.Immutable;
using System.Data;

using NQuery.Metadata;

namespace NQuery.Data;

public static class DataContextExtensions
{
    public static DataContext AddTablesAndRelations(this DataContext dataContext, DataSet dataSet)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(dataSet);

        return dataContext.AddTables(dataSet).AddRelations(dataSet);
    }

    public static DataContext AddTables(this DataContext dataContext, DataSet dataSet)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(dataSet);

        var dataTables = dataSet.Tables.OfType<DataTable>();
        return dataContext.AddTables(dataTables);
    }

    public static DataContext AddTables(this DataContext dataContext, params DataTable[] dataTables)
    {
        ThrowIfNull(dataContext);

        if (dataTables is null || dataTables.Length == 0)
            return dataContext;

        return dataContext.AddTables(dataTables.AsEnumerable());
    }

    public static DataContext AddTables(this DataContext dataContext, IEnumerable<DataTable> dataTables)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(dataTables);

        var tableDefinitions = dataTables.Select(CreateTable);
        return dataContext.AddTables(tableDefinitions);
    }

    public static DataContext AddRelations(this DataContext dataContext, DataSet dataSet)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(dataSet);

        var dataRelations = dataSet.Relations.OfType<DataRelation>();
        return dataContext.AddRelations(dataRelations);
    }

    public static DataContext AddRelations(this DataContext dataContext, params DataRelation[] dataRelations)
    {
        ThrowIfNull(dataContext);

        if (dataRelations is null || dataRelations.Length == 0)
            return dataContext;

        return dataContext.AddRelations(dataRelations.AsEnumerable());
    }

    public static DataContext AddRelations(this DataContext dataContext, IEnumerable<DataRelation> dataRelations)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(dataRelations);

        var tableRelations = dataRelations.Select(r => CreateRelation(dataContext.Tables, r)).OfType<TableRelation>();
        return dataContext.AddRelations(tableRelations);
    }

    private static DataTableDefinition CreateTable(DataTable dataTable)
    {
        return new DataTableDefinition(dataTable);
    }

    private static TableRelation? CreateRelation(IReadOnlyList<TableDefinition> tables, DataRelation dataRelation)
    {
        var parentTable = ResolveTable(tables, dataRelation.ParentTable.TableName);
        var childTable = ResolveTable(tables, dataRelation.ChildTable.TableName);

        if (parentTable is null || childTable is null)
            return null;

        var parentColumns = ResolveColumns(parentTable.Columns, dataRelation.ParentColumns);
        var childColumns = ResolveColumns(childTable.Columns, dataRelation.ChildColumns);

        if (parentColumns.Length != dataRelation.ParentColumns.Length ||
            childColumns.Length != dataRelation.ChildColumns.Length)
            return null;

        return TableRelation.Create(parentTable, parentColumns, childTable, childColumns);
    }

    private static TableDefinition? ResolveTable(IEnumerable<TableDefinition> tables, string tableName)
    {
        return tables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));
    }

    private static ImmutableArray<ColumnDefinition> ResolveColumns(IEnumerable<ColumnDefinition> columns, IEnumerable<DataColumn> dataColumns)
    {
        var columnByName = columns.ToLookup(c => c.Name, StringComparer.OrdinalIgnoreCase);
        return (from dc in dataColumns
                let c = columnByName[dc.ColumnName].FirstOrDefault()
                where c is not null
                select c).ToImmutableArray();
    }
}
