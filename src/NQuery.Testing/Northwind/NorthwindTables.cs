namespace NQuery.Northwind;

// Strongly typed rows for the Northwind tables, used by NorthwindCatalog.InstancedType. Each
// record maps one-to-one onto a Northwind table; the positional properties become the table's
// columns (their names and types must match the catalog the engine binds against).
//
// Nullability follows the actual data in Resources\Northwind.xml, not the DataSet schema (the
// schema declares every column minOccurs="0", i.e. nullable, but most columns never carry a
// null in practice). The only columns that are ever null in the data are Customer.Region/
// PostalCode/Fax, Employee.Region/ReportsTo, Order.ShipRegion/ShipPostalCode/ShippedDate, and
// Supplier.Region/Fax/HomePage.

public sealed record Category(
    int CategoryID,
    string CategoryName,
    string Description,
    byte[] Picture);

public sealed record CustomerCustomerDemo(
    string CustomerID,
    string CustomerTypeID);

public sealed record CustomerDemographic(
    string CustomerTypeID,
    string CustomerDesc);

public sealed record Customer(
    string CustomerID,
    string CompanyName,
    string ContactName,
    string ContactTitle,
    string Address,
    string City,
    string? Region,
    string? PostalCode,
    string Country,
    string Phone,
    string? Fax);

public sealed record Employee(
    int EmployeeID,
    string LastName,
    string FirstName,
    string Title,
    string TitleOfCourtesy,
    DateTime BirthDate,
    DateTime HireDate,
    string Address,
    string City,
    string? Region,
    string PostalCode,
    string Country,
    string HomePhone,
    string Extension,
    byte[] Photo,
    string Notes,
    int? ReportsTo,
    string PhotoPath);

public sealed record EmployeeTerritory(
    int EmployeeID,
    string TerritoryID);

public sealed record OrderDetail(
    int OrderID,
    int ProductID,
    decimal UnitPrice,
    short Quantity,
    float Discount);

public sealed record Order(
    int OrderID,
    string CustomerID,
    int EmployeeID,
    DateTime OrderDate,
    DateTime RequiredDate,
    DateTime? ShippedDate,
    int ShipVia,
    decimal Freight,
    string ShipName,
    string ShipAddress,
    string ShipCity,
    string? ShipRegion,
    string? ShipPostalCode,
    string ShipCountry);

public sealed record Product(
    int ProductID,
    string ProductName,
    int SupplierID,
    int CategoryID,
    string QuantityPerUnit,
    decimal UnitPrice,
    short UnitsInStock,
    short UnitsOnOrder,
    short ReorderLevel,
    bool Discontinued);

public sealed record Region(
    int RegionID,
    string RegionDescription);

public sealed record Shipper(
    int ShipperID,
    string CompanyName,
    string Phone);

public sealed record Supplier(
    int SupplierID,
    string CompanyName,
    string ContactName,
    string ContactTitle,
    string Address,
    string City,
    string? Region,
    string PostalCode,
    string Country,
    string Phone,
    string? Fax,
    string? HomePage);

public sealed record Territory(
    string TerritoryID,
    string TerritoryDescription,
    int RegionID);
