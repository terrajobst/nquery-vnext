using System.Data;

using NQuery.Data;
using NQuery.Metadata;

namespace NQuery.Northwind;

public static class NorthwindCatalog
{
    public static Catalog Instance { get; } = Catalog.Default.AddTablesAndRelationships(CreateDataSet());

    // Like Instance, but every table is backed by a strongly typed record (see NorthwindTables)
    // instead of an untyped DataRow. The data, table names, column names, and relationships are
    // identical to Instance -- only the row type differs -- so queries bind and execute the same
    // way while exercising the engine's typed (non-boxing) column readers.
    public static Catalog InstanceTyped { get; } = CreateInstanceTyped();

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

    private static Catalog CreateInstanceTyped()
    {
        // The typed rows come from NorthwindData; here we only name the tables (the lookup name
        // must match the DataSet so AddRelationships can resolve the relationships below).
        var data = NorthwindData.Instance;
        var catalog = Catalog.Default.AddTables(
            TableDefinition.Create("Categories", data.Categories),
            TableDefinition.Create("CustomerCustomerDemo", data.CustomerCustomerDemos),
            TableDefinition.Create("CustomerDemographics", data.CustomerDemographics),
            TableDefinition.Create("Customers", data.Customers),
            TableDefinition.Create("Employees", data.Employees),
            TableDefinition.Create("EmployeeTerritories", data.EmployeeTerritories),
            TableDefinition.Create("Order Details", data.OrderDetails),
            TableDefinition.Create("Orders", data.Orders),
            TableDefinition.Create("Products", data.Products),
            TableDefinition.Create("Region", data.Regions),
            TableDefinition.Create("Shippers", data.Shippers),
            TableDefinition.Create("Suppliers", data.Suppliers),
            TableDefinition.Create("Territories", data.Territories));

        // The DataSet still carries the relationship metadata; AddRelationships resolves it
        // against the typed tables we just added by matching table and column names.
        return catalog.AddRelationships(CreateDataSet());
    }
}
