using System.Data;

using NQuery.Data;

namespace NQuery;

public static class NorthwindDataContext
{
    public static readonly DataContext Instance = DataContext.Default.AddTablesAndRelations(CreateDataSet());

    // The raw Northwind data as an engine-neutral DataSet. Exposed so other engines (e.g.
    // the baseline build in the differential tests) can build their own DataContext from
    // exactly the same data this one uses.
    public static DataSet CreateDataSet()
    {
        var dataSet = new DataSet();
        using (var stream = new MemoryStream(Resources.Northwind))
            dataSet.ReadXml(stream);

        return dataSet;
    }
}