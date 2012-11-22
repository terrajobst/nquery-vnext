using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Data
{
    public static class DataContextBuilderExtensions
    {
        public static void Add(this IList<TableSymbol> tables, DataTable dataTable)
        {
            var columns = dataTable.Columns
                                   .Cast<DataColumn>()
                                   .Select(c => new ColumnSymbol(c.ColumnName, c.DataType))
                                   .ToArray();
            var table = new SchemaTableSymbol(dataTable.TableName, columns);
            tables.Add(table);
        }

        public static void AddRange(this IList<TableSymbol> tables, IEnumerable<DataTable> dataTables)
        {
            foreach (var dataTable in dataTables)
                tables.Add(dataTable);
        }

        public static void AddRange(this IList<TableSymbol> tables, DataTableCollection dataTables)
        {
            tables.AddRange(dataTables.Cast<DataTable>());
        }

        public static void Add(this IList<TableRelation> relations, DataRelation dataRelation, List<TableSymbol> tables)
        {
            var parentTable = ResolveTable(tables, dataRelation.ParentTable.TableName);
            var childTable = ResolveTable(tables, dataRelation.ChildTable.TableName);

            if (parentTable == null || childTable == null)
                return;

            var parentColumns = ResolveColumns(parentTable.Columns, dataRelation.ParentColumns);
            var childColumns = ResolveColumns(childTable.Columns, dataRelation.ChildColumns);

            if (parentColumns.Count != dataRelation.ParentColumns.Length ||
                childColumns.Count != dataRelation.ChildColumns.Length)
                return;

            var relation = new TableRelation(parentTable, parentColumns, childTable, childColumns);
            relations.Add(relation);
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

        public static void AddRange(this IList<TableRelation> relations, IEnumerable<DataRelation> dataRelations, List<TableSymbol> tables)
        {
            foreach (var dataRelation in dataRelations)
                relations.Add(dataRelation, tables);
        }

        public static void AddRange(this IList<TableRelation> relations, DataRelationCollection dataRelations, List<TableSymbol> tables)
        {
            relations.AddRange(dataRelations.Cast<DataRelation>(), tables);
        }

        public static void AddTablesAndRelations(this DataContextBuilder builder, DataSet dataSet)
        {
            builder.Tables.AddRange(dataSet.Tables);
            builder.Relations.AddRange(dataSet.Relations, builder.Tables);
        }
    }
}
