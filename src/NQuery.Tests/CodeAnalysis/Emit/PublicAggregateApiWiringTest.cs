using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Emit;
using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Optimization;
using NQuery.CodeAnalysis.Planning;
using NQuery.Metadata;

namespace NQuery.Tests.CodeAnalysis.Emit;

public sealed class PublicAggregateApiWiringTest
{
    // A custom aggregate defined purely through the public API (AggregateDefinition.Create +
    // AggregateFold) with plain System.Linq.Expressions -- multiplies the int inputs.
    private static AggregateDefinition CreateProductAggregate()
    {
        return AggregateDefinition.Create("MUL", argumentType =>
        {
            if (argumentType != typeof(int))
                return (AggregateFold?)null;

            System.Linq.Expressions.Expression<Func<int>> seed = () => 1;
            System.Linq.Expressions.Expression<Func<int, int, int>> accumulate = (acc, v) => acc * v;
            System.Linq.Expressions.Expression<Func<int, int>> result = acc => acc;
            return new AggregateFold(seed, accumulate, result);
        });
    }

    [Fact]
    public void CustomAggregate_RunsThroughEngine()
    {
        var catalog = NorthwindCatalog.Instance.AddAggregates(CreateProductAggregate());

        var ids = Run(catalog, "SELECT e.EmployeeID FROM Employees e").Select(r => (int)r[0]).ToArray();
        var expected = ids.Aggregate(1, (acc, v) => acc * v);

        var rows = Run(catalog, "SELECT MUL(e.EmployeeID) FROM Employees e");

        Assert.Single(rows);
        Assert.Equal(expected, (int)rows[0][0]);
    }

    [Fact]
    public void GenericCustomAggregate_RunsThroughEngine()
    {
        var bitOr = AggregateDefinition.Create<int, int, int>("BIT_OR",
            seed: () => 0,
            accumulate: (acc, v) => acc | v,
            result: acc => acc);
        var catalog = NorthwindCatalog.Instance.AddAggregates(bitOr);

        var ids = Run(catalog, "SELECT e.EmployeeID FROM Employees e").Select(r => (int)r[0]).ToArray();
        var expected = ids.Aggregate(0, (acc, v) => acc | v);

        var rows = Run(catalog, "SELECT BIT_OR(e.EmployeeID) FROM Employees e");

        Assert.Single(rows);
        Assert.Equal(expected, (int)rows[0][0]);
    }

    [Fact]
    public void CustomAggregate_RejectsUnsupportedType()
    {
        var catalog = NorthwindCatalog.Instance.AddAggregates(CreateProductAggregate());
        var syntaxTree = SyntaxTree.ParseQuery("SELECT MUL(e.FirstName) FROM Employees e");
        var bindingResult = Binder.Bind(syntaxTree.Root, catalog);

        // The binder returned null for string, so the aggregate reports it doesn't support the type.
        Assert.NotEmpty(bindingResult.Diagnostics);
    }

    private static List<object[]> Run(Catalog catalog, string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        var bindingResult = Binder.Bind(syntaxTree.Root, catalog);
        Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
        var boundQuery = (BoundQuery)bindingResult.BoundRoot;

        var physicalQuery = Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(boundQuery), catalog));
        var plan = Emitter.Emit(physicalQuery);

        using var iterator = plan.CreateIterator();
        iterator.Open();

        var allocation = new NQuery.CodeAnalysis.Iterators.RowBufferAllocation(null, iterator.RowBuffer, plan.OutputValueSlots);
        var entries = plan.OutputValueSlots.Select(s => allocation[s]).ToArray();

        var rows = new List<object[]>();
        while (iterator.Read())
        {
            var row = new object[entries.Length];
            for (var i = 0; i < entries.Length; i++)
                row[i] = entries[i].GetValue()!;
            rows.Add(row);
        }

        return rows;
    }
}
