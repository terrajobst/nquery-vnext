using System;
using System.Data;
using System.IO;

using NQuery.Data;

namespace NQuery
{
    public static class NorthwindDataContext
    {
        public static readonly DataContext Instance = Create();

        private static DataContext Create()
        {
            var dataSet = new DataSet();
            using (var stream = new MemoryStream(Resources.Northwind))
                dataSet.ReadXml(stream);

            return DataContext.Default.AddTablesAndRelations(dataSet);
        }
    }
}