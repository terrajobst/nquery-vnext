using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.Metadata;

namespace NQuery.Data;

public static class CatalogExtensions
{
    extension(Catalog catalog)
    {
        public Catalog AddTablesAndRelationships(DataSet dataSet)
        {
            ThrowIfNull(catalog);
            ThrowIfNull(dataSet);

            return catalog.AddTables(dataSet).AddRelationships(dataSet);
        }

        public Catalog AddTables(DataSet dataSet)
        {
            ThrowIfNull(catalog);
            ThrowIfNull(dataSet);

            var dataTables = dataSet.Tables.OfType<DataTable>();
            return catalog.AddTables(dataTables);
        }

        public Catalog AddTables(params IEnumerable<DataTable> dataTables)
        {
            ThrowIfNull(catalog);

            if (dataTables is null)
                return catalog;

            var tableDefinitions = dataTables.Select(CreateTable);
            return catalog.AddTables(tableDefinitions);
        }

        public Catalog AddRelationships(DataSet dataSet)
        {
            ThrowIfNull(catalog);
            ThrowIfNull(dataSet);

            var dataRelations = dataSet.Relations.OfType<DataRelation>();
            return catalog.AddRelationships(dataRelations);
        }

        public Catalog AddRelationships(params IEnumerable<DataRelation> dataRelations)
        {
            ThrowIfNull(catalog);

            if (dataRelations is null)
                return catalog;

            var relationships = dataRelations.Select(r => CreateRelationship(catalog.Tables, r)).OfType<RelationshipDefinition>();
            return catalog.AddRelationships(relationships);
        }
    }

    private static TableDefinition CreateTable(DataTable dataTable)
    {
        var columns = dataTable.Columns
                               .Cast<DataColumn>()
                               .Select(CreateColumn);

        return TableDefinition.Create(dataTable.TableName, dataTable.Rows, typeof(DataRow), columns);
    }

    private static readonly PropertyInfo DataRowIndexer = typeof(DataRow).GetProperty("Item", [typeof(DataColumn)])!;
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

    private static RelationshipDefinition? CreateRelationship(IReadOnlyList<TableDefinition> tables, DataRelation dataRelation)
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

        return RelationshipDefinition.Create(parentTable, parentColumns, childTable, childColumns);
    }

    private static TableDefinition? ResolveTable(IEnumerable<TableDefinition> tables, string tableName)
    {
        return tables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));
    }

    private static ImmutableArray<ColumnDefinition> ResolveColumns(IEnumerable<ColumnDefinition> columns, IEnumerable<DataColumn> dataColumns)
    {
        var columnByName = columns.ToLookup(c => c.Name, StringComparer.OrdinalIgnoreCase);
        return [.. (from dc in dataColumns
                let c = columnByName[dc.ColumnName].FirstOrDefault()
                where c is not null
                select c)];
    }
}
