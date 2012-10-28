using System;
using System.Data;
using System.IO;

using NQuery.Data;

namespace NQuery.SampleData
{
    public static class DataContextFactory
    {
        public static DataContext CreateAdventureWorksCinema()
        {
            return GetDataContext(Resources.AdventureWorksCinema);
        }

        public static DataContext CreateNorthwind()
        {
            return GetDataContext(Resources.Northwind);
        }

        public static DataContext CreatePubs()
        {
            return GetDataContext(Resources.Pubs);
        }

        private static DataContext GetDataContext(byte[] northwind)
        {
            var dataSet = new DataSet();
            using (var stream = new MemoryStream(northwind))
                dataSet.ReadXml(stream);

            var dataContexBuilder = new DataContextBuilder();
            dataContexBuilder.AddTablesAndRelations(dataSet);
            return dataContexBuilder.GetResult();
        }
    }
}