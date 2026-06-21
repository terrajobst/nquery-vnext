using System.Data;

using NQuery.Data;

namespace NQuery.Northwind;

public static class NorthwindCatalog
{
    public static Catalog Instance { get; } = Catalog.Default.AddTablesAndRelationships(CreateDataSet());

    // The raw Northwind data as an engine-neutral DataSet. Exposed so other engines (e.g.
    // the baseline build in the differential tests) can build their own Catalog from
    // exactly the same data this one uses.
    public static DataSet CreateDataSet()
    {
        var dataSet = new DataSet();
        using (var stream = new MemoryStream(Resources.Northwind))
            dataSet.ReadXml(stream);

        return dataSet;
    }
}
