using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Data
{
    public static class DataContextExtensions
    {
        public static DataContext AddTablesAndRelations(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            return dataContext.AddTables(dataSet).AddRelations(dataSet);
        }

        public static DataContext AddTables(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var dataTables = dataSet.Tables.OfType<DataTable>();
            return dataContext.AddTables(dataTables);
        }

        public static DataContext AddTables(this DataContext dataContext, params DataTable[] dataTables)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataTables == null || dataTables.Length == 0)
                return dataContext;

            return dataContext.AddTables(dataTables.AsEnumerable());
        }

        public static DataContext AddTables(this DataContext dataContext, IEnumerable<DataTable> dataTables)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataTables == null)
                throw new ArgumentNullException("dataTables");

            var tableSymbols = dataTables.Select(CreateTable);
            return dataContext.AddTables(tableSymbols);
        }

        public static DataContext AddRelations(this DataContext dataContext, DataSet dataSet)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var dataRelations = dataSet.Relations.OfType<DataRelation>();
            return dataContext.AddRelations(dataRelations);
        }

        public static DataContext AddRelations(this DataContext dataContext, params DataRelation[] dataRelations)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataRelations == null || dataRelations.Length == 0)
                return dataContext;

            return dataContext.AddRelations(dataRelations.AsEnumerable());
        }

        public static DataContext AddRelations(this DataContext dataContext, IEnumerable<DataRelation> dataRelations)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataRelations == null)
                throw new ArgumentNullException("dataRelations");

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

            if (parentTable == null || childTable == null)
                return null;

            var parentColumns = ResolveColumns(parentTable.Columns, dataRelation.ParentColumns);
            var childColumns = ResolveColumns(childTable.Columns, dataRelation.ChildColumns);

            if (parentColumns.Count != dataRelation.ParentColumns.Length ||
                childColumns.Count != dataRelation.ChildColumns.Length)
                return null;

            return new TableRelation(parentTable, parentColumns, childTable, childColumns);
        }

        private static TableSymbol ResolveTable(IEnumerable<TableSymbol> tables, string tableName)
        {
            return tables.FirstOrDefault(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));
        }

        private static IList<ColumnSymbol> ResolveColumns(IEnumerable<ColumnSymbol> columns, IEnumerable<DataColumn> dataColumns)
        {
            var columnByName = columns.ToLookup(c => c.Name, StringComparer.OrdinalIgnoreCase);
            return (from dc in dataColumns
                    let c = columnByName[dc.ColumnName].FirstOrDefault()
                    where c != null
                    select c).ToArray();
        }
    }
}
