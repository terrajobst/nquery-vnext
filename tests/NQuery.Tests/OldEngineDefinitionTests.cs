using System.Data;
using System.Linq.Expressions;
using System.Xml;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.Data;
using NQuery.Metadata;
using NQuery.Tests.Oracle;

namespace NQuery.Tests;

// Data-driven harness that replays the OLD engine's XML test definitions (from the
// "nquery-old" sibling repo) against the CURRENT engine. Each <test>.xml carries a <sql>
// query and the <expectedResults> the old engine produced (serialized as a DataSet); we run
// the same SQL through the current engine over the same three sample databases and compare
// the result set positionally.
//
// This is a TEMPORARY differential check: it reads the definitions and sample data straight out
// of the external/nquery-old submodule (nothing is copied into this repo). Tests that assert
// errors (<expectedErrors> / <expectedRuntimeError>) are skipped at enumeration time -- only
// queries with an <expectedResults> payload are replayed. A clone without the submodule checked
// out skips the whole class rather than failing it.
public class OldEngineDefinitionTests
{
    private static readonly string OldRepoRoot = FindSubmodule();

    // Path.Combine one segment at a time: the old repo's layout is written with the separators of
    // the platform running the tests, not with the backslashes it uses on Windows.
    private static readonly string DefinitionsDir = Path.Combine(OldRepoRoot, @"Src", @"NQuery.Tests", @"Definitions");
    private static readonly string SampleDataDir = Path.Combine(OldRepoRoot, @"Etc", @"Sample Data");

    // Walks up from the test binary to the repository root, the same way the sample workspace
    // tests find samples\northwind: the submodule lives outside the artifacts tree, so there is
    // nothing to copy to the output directory.
    private static string FindSubmodule()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, @"external", @"nquery-old");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        // Nothing found: the tests skip on a missing directory, so an unusable path is fine here
        // and keeps the field non-null.
        return @"external/nquery-old";
    }

    // Referenced by the theory's SkipUnless, so an absent checkout reports as skipped.
    public static bool OldRepoIsAvailable
    {
        get { return Directory.Exists(DefinitionsDir) && Directory.Exists(SampleDataDir); }
    }

    // The old engine had FIRST/LAST built-in aggregates the new engine intentionally does not;
    // we register them here (test-only) so those definition queries still bind. The aggregate
    // framework only accumulates non-null values (see AggregateCompiler), so FIRST keeps the
    // first non-null value and LAST the last -- matching the old FirstAggregator/LastAggregator
    // for the non-null columns these tests aggregate.
    //
    // Read by CreateCatalog, which the Lazy below only invokes once a test actually runs, so
    // these are always assigned by then.
    private static readonly AggregateDefinition FirstAggregate = AggregateDefinition.Create("FIRST", argumentType =>
    {
        var stateType = MakeNullable(argumentType);
        var state = Expression.Parameter(stateType, "first");
        var value = Expression.Parameter(argumentType, "value");

        // first = first is null ? value : first
        var keep = Expression.Condition(IsNull(state), Cast(value, stateType), state);
        return new AggregateFold(
            Expression.Lambda(Expression.Constant(null, stateType)),
            Expression.Lambda(keep, state, value),
            Expression.Lambda(state, state));
    });

    private static readonly AggregateDefinition LastAggregate = AggregateDefinition.Create("LAST", argumentType =>
    {
        var stateType = MakeNullable(argumentType);
        var state = Expression.Parameter(stateType, "last");
        var value = Expression.Parameter(argumentType, "value");

        // last = value
        return new AggregateFold(
            Expression.Lambda(Expression.Constant(null, stateType)),
            Expression.Lambda(Cast(value, stateType), state, value),
            Expression.Lambda(state, state));
    });

    // The current-engine catalog over the same three databases the old test fixture loaded
    // (see QueryFactory.CreateQuery in nquery-old): Northwind, the tiny JoinTables set, and
    // AdventureWorks Cinema. All three share one catalog so any query can bind its tables.
    // Lazy, not eager: loading the sample data in a static field initializer turns a missing
    // checkout into a TypeInitializationException raised from the MemberData enumeration, which
    // xUnit reports as a failure long before SkipUnless gets a chance to skip the test.
    private static readonly Lazy<Catalog> LazyCatalog = new(CreateCatalog);

    private static Catalog CreateCatalog()
    {
        return Catalog.Default
            .AddTablesAndRelationships(LoadDataSet("Northwind.xml"))
            .AddTablesAndRelationships(LoadDataSet("JoinTables.xml"))
            .AddTablesAndRelationships(LoadDataSet("AdventureWorks Cinema.xml"))
            .AddAggregates(FirstAggregate, LastAggregate);
    }

    private static Type MakeNullable(Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) is null
            ? typeof(Nullable<>).MakeGenericType(type)
            : type;
    }

    private static Expression Cast(Expression value, Type type)
    {
        return value.Type == type ? value : Expression.Convert(value, type);
    }

    private static Expression IsNull(Expression value)
    {
        if (Nullable.GetUnderlyingType(value.Type) is not null)
            return Expression.Not(Expression.Property(value, "HasValue"));

        if (!value.Type.IsValueType)
            return Expression.ReferenceEqual(value, Expression.Constant(null, value.Type));

        return Expression.Constant(false);
    }

    private static DataSet LoadDataSet(string fileName)
    {
        var dataSet = new DataSet();
        dataSet.ReadXml(Path.Combine(SampleDataDir, fileName));
        return dataSet;
    }

    public static TheoryData<string, string> TestCases()
    {
        var testCases = new TheoryData<string, string>();

        // SkipUnless can only skip test cases that exist, and a theory with no data at all is
        // reported as a failure ("No data found"). One placeholder row gives it something to
        // skip, so a machine without the old repo shows a skip rather than a red test.
        if (!OldRepoIsAvailable)
        {
            testCases.Add(@"(no old engine repo)", string.Empty);
            return testCases;
        }

        foreach (var file in Directory.EnumerateFiles(DefinitionsDir, "*.xml", SearchOption.AllDirectories).OrderBy(f => f))
        {
            var document = new XmlDocument();
            document.Load(file);

            // Skip tests that assert errors -- we only replay result-producing queries.
            if (document.SelectSingleNode("/test/expectedErrors") is not null ||
                document.SelectSingleNode("/test/expectedRuntimeError") is not null)
            {
                continue;
            }

            // Need an expected result set to compare against.
            if (document.SelectSingleNode("/test/expectedResults") is null)
                continue;

            // A nice, stable display name: "<Folder>/<File>".
            var name = Path.Combine(
                Path.GetFileName(Path.GetDirectoryName(file)!),
                Path.GetFileNameWithoutExtension(file)).Replace('\\', '/');

            // Skip the handful of tests that exercise behavior the refactored engine intentionally
            // changed (not bugs); see IntentionalEngineDifferences.
            if (IntentionalEngineDifferences.ContainsKey(name))
                continue;

            testCases.Add(name, file);
        }

        return testCases;
    }

    // Old definition tests whose queries rely on behavior the refactored engine deliberately
    // changed, so they will never match and are not worth flagging as differential failures.
    private static readonly Dictionary<string, string> IntentionalEngineDifferences = new()
    {
        // Uses the old one-word "SOUNDSLIKE" operator. There is no SOUNDSLIKE in standard SQL; the
        // engine now spells it "SOUNDS LIKE" (two words, as in MySQL), so the old syntax no longer
        // parses. (SIMILAR TO, which is standard, is supported.)
        ["CombinedTests/BetweenLikeSoundslikeSimilarToNegations"] =
            "uses the legacy one-word SOUNDSLIKE operator (now spelled SOUNDS LIKE)",

        // Mixes decimal and double in arithmetic. The old engine implicitly converted double to
        // decimal (result decimal); the refactored engine follows C# and treats decimal/double as
        // not implicitly convertible, so the expression does not bind.
        ["CombinedTests/Test2"] =
            "mixes decimal and double arithmetic, which the engine no longer converts implicitly",

        // FORMAT(x, 'C') depends on the current culture's currency formatting. The engine now
        // formats with the invariant culture, so the rendered strings differ by design.
        ["OrderByTests/DistinctSortIsAfterComputeScalar2"] =
            "FORMAT uses the invariant culture instead of the current culture",
    };

    [Theory(Skip = @"The external/nquery-old submodule is not checked out.",
            SkipUnless = nameof(OldRepoIsAvailable))]
    [MemberData(nameof(TestCases))]
    public void CurrentEngine_MatchesOldEngineResults(string name, string file)
    {
        _ = name; // present only to make the test display name meaningful.

        var document = new XmlDocument();
        document.Load(file);

        var sql = document.SelectSingleNode("/test/sql")!.InnerText;
        var expected = ReadExpectedResults(document);

        var actual = RunQuery(sql);

        // The expected rows come from the old engine's emission order, which only matters when the
        // query pins it with a top-level ORDER BY. Without one, both engines are free to return the
        // same set of rows in any order, so compare as a multiset; with one, compare positionally so
        // the ordering itself is exercised. (See RowSet, shared with the Oracle differential tests.)
        if (HasTopLevelOrderBy(sql))
            RowSet.AssertEqualOrdered(expected, actual);
        else
            RowSet.AssertEqualUnordered(expected, actual);
    }

    private static bool HasTopLevelOrderBy(string sql)
    {
        return SyntaxTree.ParseQuery(sql).Root.Root is OrderedQuerySyntax;
    }

    private static List<object[]> RunQuery(string sql)
    {
        using var reader = Query.Create(LazyCatalog.Value, sql).ExecuteReader();

        var rows = new List<object[]>();
        while (reader.Read())
        {
            var row = new object[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = RowSet.Normalize(reader[i])!;
            rows.Add(row);
        }

        return rows;
    }

    private static List<object[]> ReadExpectedResults(XmlDocument document)
    {
        var node = document.SelectSingleNode("/test/expectedResults")!;

        var dataSet = new DataSet();
        using (var stringReader = new StringReader(node.InnerXml))
        {
            if (!string.IsNullOrWhiteSpace(node.InnerXml))
                dataSet.ReadXml(stringReader);
        }

        var rows = new List<object[]>();
        if (dataSet.Tables.Count == 0)
            return rows;

        var table = dataSet.Tables[0];
        foreach (DataRow dataRow in table.Rows)
        {
            var row = new object[table.Columns.Count];
            for (var i = 0; i < row.Length; i++)
                row[i] = RowSet.Normalize(dataRow[i])!;
            rows.Add(row);
        }

        return rows;
    }
}
