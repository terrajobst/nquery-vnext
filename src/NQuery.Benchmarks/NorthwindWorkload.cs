using System.Data;
using System.Reflection;

namespace NQuery.Benchmarks;

// Engine-neutral Northwind workload shared by the benchmarks and the validator. Nothing
// here references NQuery types, so it lives on neither side of the extern-alias boundary.
//
// The data is materialized into strongly typed records up front, on purpose. The row-buffer
// refactor we are measuring stores values in typed backing arrays instead of boxing them
// into object[]. A DataSet/DataRow source hands every cell back as object (DataRow stores
// its values as object internally), so reading from one would box at the source on *both*
// engines and the per-row source cost would swamp the buffer difference we want to show.
// Reading into typed records first makes the source cheap and identical, so what's left to
// measure is the buffer work -- the thing that actually changed. The records deliberately
// keep the original Northwind column types (decimal/short/float/int), which is where the
// no-boxing win shows up.
public static class NorthwindWorkload
{
    public sealed record Customer(string CustomerID, string Country);

    public sealed record Order(int OrderID, string CustomerID);

    public sealed record OrderDetail(int OrderID, int ProductID, decimal UnitPrice, short Quantity, float Discount);

    public enum Shape
    {
        // Table scan + filter + project over the widest base table (Order Details, ~2155
        // rows of int/decimal/short/float). Isolates raw per-row buffer cost.
        Scan,

        // Inner hash join Orders x Order Details. Exercises the combined row buffer that
        // stitches the two inputs together for every matched row.
        Join,

        // Filter-free group + aggregate over Order Details. The engine sorts on the
        // grouping column (no hash aggregate yet), then stream-aggregates -- both buffer
        // value-typed columns.
        Aggregate,

        // ORDER BY over Order Details. The sort buffers every row of value-typed columns
        // into its heap, which is exactly the copy path that changed.
        Sort,

        // Three-table join (Customers x Orders x Order Details) + group + aggregate + sort:
        // a realistic report that pushes rows through every buffer-heavy operator at once.
        Report,

        // TOP ... WITH TIES over Order Details, ordered on a value-typed column. After the
        // limit row, the iterator compares each candidate's tie column against the last
        // emitted row -- the per-row tie test that used to box both values.
        TopWithTies,
    }

    public static string Sql(Shape shape) => shape switch
    {
        Shape.Scan =>
            "SELECT OrderID, ProductID, UnitPrice, Quantity, Discount " +
            "FROM [Order Details] " +
            "WHERE UnitPrice > 20",
        Shape.Join =>
            "SELECT o.OrderID, od.ProductID, od.UnitPrice, od.Quantity " +
            "FROM Orders o " +
            "INNER JOIN [Order Details] od ON o.OrderID = od.OrderID",
        Shape.Aggregate =>
            "SELECT od.ProductID, COUNT(*), SUM(od.UnitPrice * od.Quantity) " +
            "FROM [Order Details] od " +
            "GROUP BY od.ProductID",
        Shape.Sort =>
            "SELECT OrderID, ProductID, UnitPrice, Quantity " +
            "FROM [Order Details] " +
            "ORDER BY UnitPrice DESC, Quantity DESC, ProductID",
        Shape.Report =>
            "SELECT c.Country, COUNT(*), SUM(od.UnitPrice * od.Quantity) " +
            "FROM Customers c " +
            "INNER JOIN Orders o ON c.CustomerID = o.CustomerID " +
            "INNER JOIN [Order Details] od ON o.OrderID = od.OrderID " +
            "GROUP BY c.Country " +
            "ORDER BY c.Country",
        Shape.TopWithTies =>
            "SELECT TOP 100 WITH TIES OrderID, ProductID, UnitPrice " +
            "FROM [Order Details] " +
            "ORDER BY Quantity DESC",
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    public static Customer[] Customers() => _customers.Value;
    public static Order[] Orders() => _orders.Value;
    public static OrderDetail[] OrderDetails() => _orderDetails.Value;

    private static readonly Lazy<DataSet> _dataSet = new(LoadDataSet);

    private static readonly Lazy<Customer[]> _customers = new(() =>
        Rows("Customers", r => new Customer((string)r["CustomerID"], r["Country"] as string ?? "")));

    private static readonly Lazy<Order[]> _orders = new(() =>
        Rows("Orders", r => new Order((int)r["OrderID"], (string)r["CustomerID"])));

    private static readonly Lazy<OrderDetail[]> _orderDetails = new(() =>
        Rows("Order Details", r => new OrderDetail(
            (int)r["OrderID"],
            (int)r["ProductID"],
            (decimal)r["UnitPrice"],
            (short)r["Quantity"],
            (float)r["Discount"])));

    private static T[] Rows<T>(string table, Func<DataRow, T> selector)
    {
        var rows = _dataSet.Value.Tables[table]!.Rows;
        var result = new T[rows.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = selector(rows[i]);
        return result;
    }

    private static DataSet LoadDataSet()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Northwind.xml")
            ?? throw new InvalidOperationException("Embedded resource 'Northwind.xml' was not found.");

        var dataSet = new DataSet();
        dataSet.ReadXml(stream);
        return dataSet;
    }
}
