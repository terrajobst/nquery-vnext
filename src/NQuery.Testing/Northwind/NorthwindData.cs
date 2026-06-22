using System.Collections.Immutable;
using System.Data;

namespace NQuery.Northwind;

// The single source of strongly typed Northwind rows, materialized once from the embedded
// Resources\Northwind.xml into immutable arrays. NorthwindCatalog.InstanceTyped builds its
// tables from these, and other engines (e.g. the benchmark baseline) can feed the very same
// rows to their own catalogs so a comparison runs over byte-for-byte identical data.
//
// The DataSet used to load the data is only a local in the constructor: once the rows are
// materialized it is discarded, so the singleton holds nothing but the immutable arrays.
//
// The rows keep the original Northwind column types (decimal/short/float/int/DateTime) and the
// nullability of the actual data rather than the DataSet schema -- see NorthwindTables for the
// columns that are ever null.
public sealed class NorthwindData
{
    public static NorthwindData Instance { get; } = new();

    private NorthwindData()
    {
        var dataSet = NorthwindCatalog.CreateDataSet();

        Categories = dataSet.Rows("Categories", r => new Category(
            r.Int("CategoryID"),
            r.Str("CategoryName"),
            r.Str("Description"),
            r.Bytes("Picture")));

        CustomerCustomerDemos = dataSet.Rows("CustomerCustomerDemo", r => new CustomerCustomerDemo(
            r.Str("CustomerID"),
            r.Str("CustomerTypeID")));

        CustomerDemographics = dataSet.Rows("CustomerDemographics", r => new CustomerDemographic(
            r.Str("CustomerTypeID"),
            r.Str("CustomerDesc")));

        Customers = dataSet.Rows("Customers", r => new Customer(
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
            r.StrN("Fax")));

        Employees = dataSet.Rows("Employees", r => new Employee(
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
            r.Str("PhotoPath")));

        EmployeeTerritories = dataSet.Rows("EmployeeTerritories", r => new EmployeeTerritory(
            r.Int("EmployeeID"),
            r.Str("TerritoryID")));

        OrderDetails = dataSet.Rows("Order Details", r => new OrderDetail(
            r.Int("OrderID"),
            r.Int("ProductID"),
            r.Dec("UnitPrice"),
            r.Short("Quantity"),
            r.Flt("Discount")));

        Orders = dataSet.Rows("Orders", r => new Order(
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
            r.Str("ShipCountry")));

        Products = dataSet.Rows("Products", r => new Product(
            r.Int("ProductID"),
            r.Str("ProductName"),
            r.Int("SupplierID"),
            r.Int("CategoryID"),
            r.Str("QuantityPerUnit"),
            r.Dec("UnitPrice"),
            r.Short("UnitsInStock"),
            r.Short("UnitsOnOrder"),
            r.Short("ReorderLevel"),
            r.Bool("Discontinued")));

        Regions = dataSet.Rows("Region", r => new Region(
            r.Int("RegionID"),
            r.Str("RegionDescription")));

        Shippers = dataSet.Rows("Shippers", r => new Shipper(
            r.Int("ShipperID"),
            r.Str("CompanyName"),
            r.Str("Phone")));

        Suppliers = dataSet.Rows("Suppliers", r => new Supplier(
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
            r.StrN("HomePage")));

        Territories = dataSet.Rows("Territories", r => new Territory(
            r.Str("TerritoryID"),
            r.Str("TerritoryDescription"),
            r.Int("RegionID")));
    }

    public ImmutableArray<Category> Categories { get; }
    public ImmutableArray<CustomerCustomerDemo> CustomerCustomerDemos { get; }
    public ImmutableArray<CustomerDemographic> CustomerDemographics { get; }
    public ImmutableArray<Customer> Customers { get; }
    public ImmutableArray<Employee> Employees { get; }
    public ImmutableArray<EmployeeTerritory> EmployeeTerritories { get; }
    public ImmutableArray<OrderDetail> OrderDetails { get; }
    public ImmutableArray<Order> Orders { get; }
    public ImmutableArray<Product> Products { get; }
    public ImmutableArray<Region> Regions { get; }
    public ImmutableArray<Shipper> Shippers { get; }
    public ImmutableArray<Supplier> Suppliers { get; }
    public ImmutableArray<Territory> Territories { get; }
}

// Projection helpers used only to turn the Northwind DataSet into the strongly typed records
// above.
file static class DataExtensions
{
    extension(DataSet dataSet)
    {
        // Projects every DataRow in the named table through the selector.
        public ImmutableArray<T> Rows<T>(string table, Func<DataRow, T> selector)
        {
            var rows = dataSet.Tables[table]!.Rows;
            var builder = ImmutableArray.CreateBuilder<T>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
                builder.Add(selector(rows[i]));

            return builder.MoveToImmutable();
        }
    }

    // Typed cell accessors over DataRow. The "N" suffix reads a column that can be SQL NULL into
    // a nullable target; the unsuffixed readers assume the column is never null in the data.
    extension(DataRow row)
    {
        public string Str(string column) => (string)row[column];
        public string? StrN(string column) => row.IsNull(column) ? null : (string)row[column];
        public byte[] Bytes(string column) => (byte[])row[column];
        public int Int(string column) => (int)row[column];
        public int? IntN(string column) => row.IsNull(column) ? null : (int)row[column];
        public short Short(string column) => (short)row[column];
        public decimal Dec(string column) => (decimal)row[column];
        public float Flt(string column) => (float)row[column];
        public bool Bool(string column) => (bool)row[column];
        public DateTime Date(string column) => (DateTime)row[column];
        public DateTime? DateN(string column) => row.IsNull(column) ? null : (DateTime)row[column];
    }
}
