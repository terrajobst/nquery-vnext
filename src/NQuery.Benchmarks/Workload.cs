namespace NQuery.Benchmarks;

// Engine-neutral workload shared by the benchmarks and the validator. Nothing here
// references NQuery types, so it lives on neither side of the extern-alias boundary.
public static class Workload
{
    public sealed record Number(int Id, double Value, string Category);

    public enum Shape
    {
        // Scan + filter + project: isolates raw per-row cost (no sort/aggregate).
        Scan,

        // Filter + group + aggregate: the engine sorts on the grouping columns
        // (no hash aggregate yet), so this is where the sort gap shows up.
        Aggregate,
    }

    private static readonly string[] Categories = ["A", "B", "C", "D"];

    public static string Sql(Shape shape) => shape switch
    {
        Shape.Scan => "SELECT Id, Value FROM Numbers WHERE Value > 100.0",
        Shape.Aggregate => "SELECT Category, COUNT(*), SUM(Value) FROM Numbers WHERE Value > 100.0 GROUP BY Category",
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    public static Number[] BuildRows(int count)
    {
        var rows = new Number[count];
        for (var i = 0; i < count; i++)
            rows[i] = new Number(i, i % 250, Categories[i % Categories.Length]);
        return rows;
    }
}
