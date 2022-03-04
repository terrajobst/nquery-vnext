using System.Collections.Immutable;
using System.Data;

using NQuery.Symbols;

namespace NQuery.Data
{
    public static class DataContextExtensions
    {
        public static DataContext AddTablesAndRelations(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));

            return dataContext.AddTables(dataSet).AddRelations(dataSet);
        }

        public static DataContext AddTables(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));

            var dataTables = dataSet.Tables.OfType<DataTable>();
            return dataContext.AddTables(dataTables);
        }

        public static DataContext AddTables(this DataContext dataContext, params DataTable[] dataTables)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataTables is null || dataTables.Length == 0)
                return dataContext;

            return dataContext.AddTables(dataTables.AsEnumerable());
        }

        public static DataContext AddTables(this DataContext dataContext, IEnumerable<DataTable> dataTables)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataTables is null)
                throw new ArgumentNullException(nameof(dataTables));

            var tableSymbols = dataTables.Select(CreateTable);
            return dataContext.AddTables(tableSymbols);
        }

        public static DataContext AddRelations(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));

            var dataRelations = dataSet.Relations.OfType<DataRelation>();
            return dataContext.AddRelations(dataRelations);
        }

        public static DataContext AddRelations(this DataContext dataContext, params DataRelation[] dataRelations)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataRelations is null || dataRelations.Length == 0)
                return dataContext;

            return dataContext.AddRelations(dataRelations.AsEnumerable());
        }

        public static DataContext AddRelations(this DataContext dataContext, IEnumerable<DataRelation> dataRelations)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (dataRelations is null)
                throw new ArgumentNullException(nameof(dataRelations));

            var tableRelations = dataRelations.Select(r => CreateRelation(dataContext.Tables, r));
            return dataContext.AddRelations(tableRelations);
        }

        private static SchemaTableSymbol CreateTable(DataTable dataTable)
        {
            var tableDefinition = new DataTableDefinition(dataTable);
            return new SchemaTableSymbol(tableDefinition);
        }

        private static TableRelation CreateRelation(IReadOnlyList<TableSymbol> tables, DataRelation dataRelation)
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

            return new TableRelation(parentTable, parentColumns, childTable, childColumns);
        }

        private static TableSymbol ResolveTable(IEnumerable<TableSymbol> tables, string tableName)
        {
            return tables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));
        }

        private static ImmutableArray<ColumnSymbol> ResolveColumns(IEnumerable<ColumnSymbol> columns, IEnumerable<DataColumn> dataColumns)
        {
            var columnByName = columns.ToLookup(c => c.Name, StringComparer.OrdinalIgnoreCase);
            return (from dc in dataColumns
                    let c = columnByName[dc.ColumnName].FirstOrDefault()
                    where c is not null
                    select c).ToImmutableArray();
        }
    }
}