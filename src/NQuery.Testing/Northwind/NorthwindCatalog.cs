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
        var dataSet = CreateDataSet();

        var catalog = Catalog.Default.AddTables(
            dataSet.Table("Categories", r => new Category(
                r.Int("CategoryID"),
                r.Str("CategoryName"),
                r.Str("Description"),
                r.Bytes("Picture"))),
            dataSet.Table("CustomerCustomerDemo", r => new CustomerCustomerDemo(
                r.Str("CustomerID"),
                r.Str("CustomerTypeID"))),
            dataSet.Table("CustomerDemographics", r => new CustomerDemographic(
                r.Str("CustomerTypeID"),
                r.Str("CustomerDesc"))),
            dataSet.Table("Customers", r => new Customer(
                r.Str("CustomerID"),
                r.Str("CompanyName"),
                r.Str("ContactName"),
                r.Str("ContactTitle"),
                r.Str("Address"),
                r.Str("City"),
                r.StrN("Region"),
                r.StrN("PostalCode"),
                r.Str("Country"),
                r.Str("Phone"),
                r.StrN("Fax"))),
            dataSet.Table("Employees", r => new Employee(
                r.Int("EmployeeID"),
                r.Str("LastName"),
                r.Str("FirstName"),
                r.Str("Title"),
                r.Str("TitleOfCourtesy"),
                r.Date("BirthDate"),
                r.Date("HireDate"),
                r.Str("Address"),
                r.Str("City"),
                r.StrN("Region"),
                r.Str("PostalCode"),
                r.Str("Country"),
                r.Str("HomePhone"),
                r.Str("Extension"),
                r.Bytes("Photo"),
                r.Str("Notes"),
                r.IntN("ReportsTo"),
                r.Str("PhotoPath"))),
            dataSet.Table("EmployeeTerritories", r => new EmployeeTerritory(
                r.Int("EmployeeID"),
                r.Str("TerritoryID"))),
            dataSet.Table("Order Details", r => new OrderDetail(
                r.Int("OrderID"),
                r.Int("ProductID"),
                r.Dec("UnitPrice"),
                r.Short("Quantity"),
                r.Flt("Discount"))),
            dataSet.Table("Orders", r => new Order(
                r.Int("OrderID"),
                r.Str("CustomerID"),
                r.Int("EmployeeID"),
                r.Date("OrderDate"),
                r.Date("RequiredDate"),
                r.DateN("ShippedDate"),
                r.Int("ShipVia"),
                r.Dec("Freight"),
                r.Str("ShipName"),
                r.Str("ShipAddress"),
                r.Str("ShipCity"),
                r.StrN("ShipRegion"),
                r.StrN("ShipPostalCode"),
                r.Str("ShipCountry"))),
            dataSet.Table("Products", r => new Product(
                r.Int("ProductID"),
                r.Str("ProductName"),
                r.Int("SupplierID"),
                r.Int("CategoryID"),
                r.Str("QuantityPerUnit"),
                r.Dec("UnitPrice"),
                r.Short("UnitsInStock"),
                r.Short("UnitsOnOrder"),
                r.Short("ReorderLevel"),
                r.Bool("Discontinued"))),
            dataSet.Table("Region", r => new Region(
                r.Int("RegionID"),
                r.Str("RegionDescription"))),
            dataSet.Table("Shippers", r => new Shipper(
                r.Int("ShipperID"),
                r.Str("CompanyName"),
                r.Str("Phone"))),
            dataSet.Table("Suppliers", r => new Supplier(
                r.Int("SupplierID"),
                r.Str("CompanyName"),
                r.Str("ContactName"),
                r.Str("ContactTitle"),
                r.Str("Address"),
                r.Str("City"),
                r.StrN("Region"),
                r.Str("PostalCode"),
                r.Str("Country"),
                r.Str("Phone"),
                r.StrN("Fax"),
                r.StrN("HomePage"))),
            dataSet.Table("Territories", r => new Territory(
                r.Str("TerritoryID"),
                r.Str("TerritoryDescription"),
                r.Int("RegionID"))));

        // The DataSet still carries the relationship metadata; AddRelationships resolves it
        // against the typed tables we just added by matching table and column names.
        return catalog.AddRelationships(dataSet);
    }

    // Builds a strongly typed TableDefinition from a DataSet table: projects every DataRow in
    // the named table through the selector and names the table after that same lookup name.
    extension(DataSet dataSet)
    {
        private TableDefinition Table<T>(string name, Func<DataRow, T> selector)
        {
            var rows = dataSet.Tables[name]!.Rows;
            var result = new T[rows.Count];
            for (var i = 0; i < rows.Count; i++)
                result[i] = selector(rows[i]);

            return TableDefinition.Create(name, result);
        }
    }

    // Typed cell accessors over DataRow, used only to project the Northwind DataSet into the
    // strongly typed records above. The "N" suffix reads a column that can be SQL NULL into a
    // nullable target; the unsuffixed readers assume the column is never null in the data.
    extension(DataRow row)
    {
        private string Str(string column) => (string)row[column];
        private string? StrN(string column) => row.IsNull(column) ? null : (string)row[column];
        private byte[] Bytes(string column) => (byte[])row[column];
        private int Int(string column) => (int)row[column];
        private int? IntN(string column) => row.IsNull(column) ? null : (int)row[column];
        private short Short(string column) => (short)row[column];
        private decimal Dec(string column) => (decimal)row[column];
        private float Flt(string column) => (float)row[column];
        private bool Bool(string column) => (bool)row[column];
        private DateTime Date(string column) => (DateTime)row[column];
        private DateTime? DateN(string column) => row.IsNull(column) ? null : (DateTime)row[column];
    }
}
