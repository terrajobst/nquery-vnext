using System;
using System.Collections;
using System.Linq;

using NQuery.Language;
using NQuery.Language.Symbols;

namespace NQuery.SampleData
{
    public static class NorthwindDataContext
    {
        public static DataContext Create()
        {
            var tables = GetSchemaTables();

            var builder = new DataContextBuilder();
            builder.Tables.AddRange(tables);
            builder.Variables.Add(new VariableSymbol("Id", typeof(int)));

            var categoriesTable = tables.Single(t => t.Name == "Categories");
            var customerCustomerDemoTable = tables.Single(t => t.Name == "CustomerCustomerDemo");
            var customerDemographicsTable = tables.Single(t => t.Name == "CustomerDemographics");
            var customersTable = tables.Single(t => t.Name == "Customers");
            var employeesTable = tables.Single(t => t.Name == "Employees");
            var employeeTerritoriesTable = tables.Single(t => t.Name == "EmployeeTerritories");
            var orderDetailsTable = tables.Single(t => t.Name == "Order Details");
            var ordersTable = tables.Single(t => t.Name == "Orders");
            var productsTable = tables.Single(t => t.Name == "Products");
            var regionTable = tables.Single(t => t.Name == "Region");
            var shippersTable = tables.Single(t => t.Name == "Shippers");
            var suppliersTable = tables.Single(t => t.Name == "Suppliers");
            var territoriesTable = tables.Single(t => t.Name == "Territories");
            builder.Relations.Add(new TableRelation(regionTable, new[] { regionTable.Columns.Single(c => c.Name == "RegionID") }, territoriesTable, new[] { territoriesTable.Columns.Single(c => c.Name == "RegionID") }));
            builder.Relations.Add(new TableRelation(categoriesTable, new[] { categoriesTable.Columns.Single(c => c.Name == "CategoryID") }, productsTable, new[] { productsTable.Columns.Single(c => c.Name == "CategoryID") }));
            builder.Relations.Add(new TableRelation(suppliersTable, new[] { suppliersTable.Columns.Single(c => c.Name == "SupplierID") }, productsTable, new[] { productsTable.Columns.Single(c => c.Name == "SupplierID") }));
            builder.Relations.Add(new TableRelation(shippersTable, new[] { shippersTable.Columns.Single(c => c.Name == "ShipperID") }, ordersTable, new[] { ordersTable.Columns.Single(c => c.Name == "ShipVia") }));
            builder.Relations.Add(new TableRelation(employeesTable, new[] { employeesTable.Columns.Single(c => c.Name == "EmployeeID") }, employeesTable, new[] { employeesTable.Columns.Single(c => c.Name == "ReportsTo") }));
            builder.Relations.Add(new TableRelation(employeesTable, new[] { employeesTable.Columns.Single(c => c.Name == "EmployeeID") }, ordersTable, new[] { ordersTable.Columns.Single(c => c.Name == "EmployeeID") }));
            builder.Relations.Add(new TableRelation(customersTable, new[] { customersTable.Columns.Single(c => c.Name == "CustomerID") }, ordersTable, new[] { ordersTable.Columns.Single(c => c.Name == "CustomerID") }));
            builder.Relations.Add(new TableRelation(productsTable, new[] { productsTable.Columns.Single(c => c.Name == "ProductID") }, orderDetailsTable, new[] { orderDetailsTable.Columns.Single(c => c.Name == "ProductID") }));
            builder.Relations.Add(new TableRelation(ordersTable, new[] { ordersTable.Columns.Single(c => c.Name == "OrderID") }, orderDetailsTable, new[] { orderDetailsTable.Columns.Single(c => c.Name == "OrderID") }));
            builder.Relations.Add(new TableRelation(territoriesTable, new[] { territoriesTable.Columns.Single(c => c.Name == "TerritoryID") }, employeeTerritoriesTable, new[] { employeeTerritoriesTable.Columns.Single(c => c.Name == "TerritoryID") }));
            builder.Relations.Add(new TableRelation(employeesTable, new[] { employeesTable.Columns.Single(c => c.Name == "EmployeeID") }, employeeTerritoriesTable, new[] { employeeTerritoriesTable.Columns.Single(c => c.Name == "EmployeeID") }));
            builder.Relations.Add(new TableRelation(customersTable, new[] { customersTable.Columns.Single(c => c.Name == "CustomerID") }, customerCustomerDemoTable, new[] { customerCustomerDemoTable.Columns.Single(c => c.Name == "CustomerID") }));
            builder.Relations.Add(new TableRelation(customerDemographicsTable, new[] { customerDemographicsTable.Columns.Single(c => c.Name == "CustomerTypeID") }, customerCustomerDemoTable, new[] { customerCustomerDemoTable.Columns.Single(c => c.Name == "CustomerTypeID") }));

            return builder.GetResult();
        }

        private static SchemaTableSymbol[] GetSchemaTables()
        {
            return new[]
                       {
                           new SchemaTableSymbol("Categories", new[]
                                                                   {
                                                                       new ColumnSymbol("CategoryID", typeof (int)),
                                                                       new ColumnSymbol("CategoryName", typeof (string)),
                                                                       new ColumnSymbol("Description", typeof (string)),
                                                                       new ColumnSymbol("Picture", typeof (byte[]))
                                                                   }, typeof (IEnumerable)),
                           new SchemaTableSymbol("CustomerCustomerDemo", new[]
                                                                             {
                                                                                 new ColumnSymbol("CustomerID", typeof (string)),
                                                                                 new ColumnSymbol("CustomerTypeID", typeof (string))
                                                                             }, typeof (IEnumerable)),
                           new SchemaTableSymbol("CustomerDemographics", new[]
                                                                             {
                                                                                 new ColumnSymbol("CustomerTypeID", typeof (string)),
                                                                                 new ColumnSymbol("CustomerDesc", typeof (string))
                                                                             }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Customers", new[]
                                                                  {
                                                                      new ColumnSymbol("CustomerID", typeof (string)),
                                                                      new ColumnSymbol("CompanyName", typeof (string)),
                                                                      new ColumnSymbol("ContactName", typeof (string)),
                                                                      new ColumnSymbol("ContactTitle", typeof (string)),
                                                                      new ColumnSymbol("Address", typeof (string)),
                                                                      new ColumnSymbol("City", typeof (string)),
                                                                      new ColumnSymbol("Region", typeof (string)),
                                                                      new ColumnSymbol("PostalCode", typeof (string)),
                                                                      new ColumnSymbol("Country", typeof (string)),
                                                                      new ColumnSymbol("Phone", typeof (string)),
                                                                      new ColumnSymbol("Fax", typeof (string))
                                                                  }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Employees", new[]
                                                                  {
                                                                      new ColumnSymbol("EmployeeID", typeof (int)),
                                                                      new ColumnSymbol("LastName", typeof (string)),
                                                                      new ColumnSymbol("FirstName", typeof (string)),
                                                                      new ColumnSymbol("Title", typeof (string)),
                                                                      new ColumnSymbol("TitleOfCourtesy", typeof (string)),
                                                                      new ColumnSymbol("BirthDate", typeof (DateTime)),
                                                                      new ColumnSymbol("HireDate", typeof (DateTime)),
                                                                      new ColumnSymbol("Address", typeof (string)),
                                                                      new ColumnSymbol("City", typeof (string)),
                                                                      new ColumnSymbol("Region", typeof (string)),
                                                                      new ColumnSymbol("PostalCode", typeof (string)),
                                                                      new ColumnSymbol("Country", typeof (string)),
                                                                      new ColumnSymbol("HomePhone", typeof (string)),
                                                                      new ColumnSymbol("Extension", typeof (string)),
                                                                      new ColumnSymbol("Photo", typeof (byte[])),
                                                                      new ColumnSymbol("Notes", typeof (string)),
                                                                      new ColumnSymbol("ReportsTo", typeof (int)),
                                                                      new ColumnSymbol("PhotoPath", typeof (string))
                                                                  }, typeof (IEnumerable)),
                           new SchemaTableSymbol("EmployeeTerritories", new[]
                                                                            {
                                                                                new ColumnSymbol("EmployeeID", typeof (int)),
                                                                                new ColumnSymbol("TerritoryID", typeof (string))
                                                                            }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Order Details", new[]
                                                                      {
                                                                          new ColumnSymbol("OrderID", typeof (int)),
                                                                          new ColumnSymbol("ProductID", typeof (int)),
                                                                          new ColumnSymbol("UnitPrice", typeof (Decimal)),
                                                                          new ColumnSymbol("Quantity", typeof (Int16)),
                                                                          new ColumnSymbol("Discount", typeof (Single))
                                                                      }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Orders", new[]
                                                               {
                                                                   new ColumnSymbol("OrderID", typeof (int)),
                                                                   new ColumnSymbol("CustomerID", typeof (string)),
                                                                   new ColumnSymbol("EmployeeID", typeof (int)),
                                                                   new ColumnSymbol("OrderDate", typeof (DateTime)),
                                                                   new ColumnSymbol("RequiredDate", typeof (DateTime)),
                                                                   new ColumnSymbol("ShippedDate", typeof (DateTime)),
                                                                   new ColumnSymbol("ShipVia", typeof (int)),
                                                                   new ColumnSymbol("Freight", typeof (Decimal)),
                                                                   new ColumnSymbol("ShipName", typeof (string)),
                                                                   new ColumnSymbol("ShipAddress", typeof (string)),
                                                                   new ColumnSymbol("ShipCity", typeof (string)),
                                                                   new ColumnSymbol("ShipRegion", typeof (string)),
                                                                   new ColumnSymbol("ShipPostalCode", typeof (string)),
                                                                   new ColumnSymbol("ShipCountry", typeof (string))
                                                               }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Products", new[]
                                                                 {
                                                                     new ColumnSymbol("ProductID", typeof (int)),
                                                                     new ColumnSymbol("ProductName", typeof (string)),
                                                                     new ColumnSymbol("SupplierID", typeof (int)),
                                                                     new ColumnSymbol("CategoryID", typeof (int)),
                                                                     new ColumnSymbol("QuantityPerUnit", typeof (string)),
                                                                     new ColumnSymbol("UnitPrice", typeof (Decimal)),
                                                                     new ColumnSymbol("UnitsInStock", typeof (Int16)),
                                                                     new ColumnSymbol("UnitsOnOrder", typeof (Int16)),
                                                                     new ColumnSymbol("ReorderLevel", typeof (Int16)),
                                                                     new ColumnSymbol("Discontinued", typeof (Boolean))
                                                                 }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Region", new[]
                                                               {
                                                                   new ColumnSymbol("RegionID", typeof (int)),
                                                                   new ColumnSymbol("RegionDescription", typeof (string))
                                                               }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Shippers", new[]
                                                                 {
                                                                     new ColumnSymbol("ShipperID", typeof (int)),
                                                                     new ColumnSymbol("CompanyName", typeof (string)),
                                                                     new ColumnSymbol("Phone", typeof (string))
                                                                 }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Suppliers", new[]
                                                                  {
                                                                      new ColumnSymbol("SupplierID", typeof (int)),
                                                                      new ColumnSymbol("CompanyName", typeof (string)),
                                                                      new ColumnSymbol("ContactName", typeof (string)),
                                                                      new ColumnSymbol("ContactTitle", typeof (string)),
                                                                      new ColumnSymbol("Address", typeof (string)),
                                                                      new ColumnSymbol("City", typeof (string)),
                                                                      new ColumnSymbol("Region", typeof (string)),
                                                                      new ColumnSymbol("PostalCode", typeof (string)),
                                                                      new ColumnSymbol("Country", typeof (string)),
                                                                      new ColumnSymbol("Phone", typeof (string)),
                                                                      new ColumnSymbol("Fax", typeof (string)),
                                                                      new ColumnSymbol("HomePage", typeof (string))
                                                                  }, typeof (IEnumerable)),
                           new SchemaTableSymbol("Territories", new[]
                                                                    {
                                                                        new ColumnSymbol("TerritoryID", typeof (string)),
                                                                        new ColumnSymbol("TerritoryDescription", typeof (string)),
                                                                        new ColumnSymbol("RegionID", typeof (int))
                                                                    }, typeof (IEnumerable))
                       };
        }

    }
}