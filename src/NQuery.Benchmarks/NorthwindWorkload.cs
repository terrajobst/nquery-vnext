namespace NQuery.Benchmarks;

// The benchmark query shapes against the real Northwind schema. The data itself lives in
// NQuery.Testing (NQuery.Northwind.NorthwindData), so both engines run over identical rows;
// this file only owns the SQL we measure.
public static class NorthwindWorkload
{
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
}
