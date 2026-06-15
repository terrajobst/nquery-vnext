using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

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

    private static TableDefinition CreateTable(DataTable dataTable)
    {
        var columns = dataTable.Columns
                               .Cast<DataColumn>()
                               .Select(CreateColumn);

        return TableDefinition.Create(dataTable.TableName, dataTable.Rows, typeof(DataRow), columns);
    }

    private static readonly PropertyInfo DataRowIndexer = typeof(DataRow).GetProperty("Item", new[] { typeof(DataColumn) })!;
    private static readonly PropertyInfo NullableIsNull = typeof(INullable).GetProperty("IsNull")!;

    private static ColumnDefinition CreateColumn(DataColumn column)
    {
        // DataRow exposes nulls as DBNull and typed nulls as INullable; the query engine works
        // in terms of CLR null, so unwrap both -- reading the cell once via a local:
        //
        //   var v = row[column];
        //   INullable n;
        //   return v is DBNull                              ? null
        //        : (n = v as INullable) is not null && n.IsNull ? null
        //        : v;
        var row = Expression.Parameter(typeof(DataRow), "row");
        var v = Expression.Variable(typeof(object), "v");
        var n = Expression.Variable(typeof(INullable), "n");
        var objectNull = Expression.Constant(null, typeof(object));

        var body = Expression.Block(
            typeof(object),
            new[] { v, n },
            Expression.Assign(
                v,
                Expression.MakeIndex(row, DataRowIndexer, new[] { Expression.Constant(column) })),
            Expression.Condition(
                Expression.TypeIs(v, typeof(DBNull)),
                objectNull,
                Expression.Condition(
                    Expression.AndAlso(
                        Expression.NotEqual(
                            Expression.Assign(n, Expression.TypeAs(v, typeof(INullable))),
                            objectNull),
                        Expression.Property(n, NullableIsNull)),
                    objectNull,
                    v)));

        var accessor = Expression.Lambda<Func<DataRow, object>>(body, row);
        return ColumnDefinition.Create<DataRow>(column.ColumnName, column.DataType, accessor);
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
