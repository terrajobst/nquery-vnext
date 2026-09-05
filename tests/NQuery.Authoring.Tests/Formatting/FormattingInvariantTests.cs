using System.Collections.Immutable;

using NQuery.Authoring.Formatting;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.Formatting;

// The formatter has an unusually strong oracle: rewriting whitespace must not change what the text
// parses to, and formatting formatted text must change nothing. Both hold for every query and every
// option set, so they're asserted over a corpus rather than case by case.
public class FormattingInvariantTests : FormattingTests
{
    [Theory]
    [MemberData(nameof(GetQueries))]
    public void Formatting_PreservesTheParse(string query)
    {
        foreach (var options in GetAllOptions())
        {
            var document = DocumentFactory.CreateQuery(query);
            var service = document.Services.GetService<FormattingService>();
            var formatted = service.Format(document, options);

            var before = document.GetSyntaxTree().Root;
            var after = formatted.GetSyntaxTree().Root;

            Assert.True(before.IsEquivalentTo(after), $"Not equivalent:\n{formatted.Text.GetText()}");
        }
    }

    [Theory]
    [MemberData(nameof(GetQueries))]
    public void Formatting_IsIdempotent(string query)
    {
        foreach (var options in GetAllOptions())
        {
            var document = DocumentFactory.CreateQuery(query);
            var service = document.Services.GetService<FormattingService>();

            var once = service.Format(document, options);
            var twice = service.Format(once, options);

            Assert.Equal(once.Text.GetText(), twice.Text.GetText());
        }
    }

    [Theory]
    [MemberData(nameof(GetQueries))]
    public void Formatting_FormattedTextProducesNoChanges(string query)
    {
        foreach (var options in GetAllOptions())
        {
            var document = DocumentFactory.CreateQuery(query);
            var service = document.Services.GetService<FormattingService>();

            var formatted = service.Format(document, options);
            var changes = service.GetChanges(formatted, options);

            Assert.Empty(changes);
        }
    }

    [Theory]
    [MemberData(nameof(GetQueries))]
    public void Formatting_KeepsEveryComment(string query)
    {
        var document = DocumentFactory.CreateQuery(query);
        var service = document.Services.GetService<FormattingService>();
        var comments = GetComments(document.GetSyntaxTree());

        foreach (var options in GetAllOptions())
        {
            var formatted = service.Format(document, options);
            Assert.Equal(comments, GetComments(formatted.GetSyntaxTree()));
        }
    }

    private static ImmutableArray<string> GetComments(SyntaxTree syntaxTree)
    {
        return [.. from token in syntaxTree.Root.DescendantTokens()
                   from trivia in token.LeadingTrivia.Concat(token.TrailingTrivia)
                   where trivia.Kind.IsComment()
                   select trivia.Text];
    }

    private static IEnumerable<FormattingOptions> GetAllOptions()
    {
        yield return GetOptions(FormattingOptions.Tabular);
        yield return GetOptions(FormattingOptions.Stacked);
        yield return GetOptions(FormattingOptions.Compact);
        yield return GetOptions(FormattingOptions.Tabular with { MaxLineLength = 30 });
        yield return GetOptions(FormattingOptions.Stacked with { MaxLineLength = 30, IndentSize = 2 });
        yield return GetOptions(FormattingOptions.Tabular with
        {
            Keywords = Casing.Lower,
            Identifiers = IdentifierQuoting.WhenRequired,
            On = OnPlacement.OwnLine,
            Joins = JoinIndentation.AtFromLevel
        });
    }

    public static TheoryData<string> GetQueries()
    {
        return
        [
            @"SELECT 1",
            @"SELECT * FROM Employees",
            @"SELECT e.FirstName, e.LastName, e.City FROM Employees e WHERE e.City = 'London'",
            @"SELECT DISTINCT TOP 10 WITH TIES e.City FROM Employees e ORDER BY e.City DESC",
            @"SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID",
            @"SELECT * FROM Employees e LEFT OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID AND et.TerritoryID > 0",
            @"SELECT * FROM Employees e CROSS JOIN Territories t",
            @"SELECT * FROM Employees e CROSS APPLY (SELECT * FROM Territories t) AS D",
            @"SELECT * FROM (SELECT * FROM Employees) AS D",
            @"WITH Emps AS (SELECT * FROM Employees) SELECT * FROM Emps",
            @"WITH Emps (Id) AS (SELECT e.EmployeeID FROM Employees e), Others AS (SELECT 1) SELECT * FROM Emps",
            @"SELECT 1 FROM Employees UNION ALL SELECT 2 FROM Employees",
            @"SELECT 1 FROM Employees INTERSECT SELECT 2 FROM Employees EXCEPT SELECT 3 FROM Employees",
            @"SELECT COUNT(*), e.Country FROM Employees e GROUP BY e.Country HAVING COUNT(*) > 1 ORDER BY 1 DESC",
            @"SELECT CASE WHEN e.City = 'London' THEN 1 WHEN e.City = 'Paris' THEN 2 ELSE 3 END FROM Employees e",
            @"SELECT CASE e.City WHEN 'London' THEN 1 ELSE 2 END FROM Employees e",
            @"SELECT (SELECT COUNT(*) FROM Employees x) FROM Employees e",
            @"SELECT * FROM Employees e WHERE EXISTS (SELECT * FROM Territories t)",
            @"SELECT * FROM Employees e WHERE e.EmployeeID IN (SELECT x.EmployeeID FROM Employees x)",
            @"SELECT * FROM Employees e WHERE e.City IN ('London', 'Paris', 'Berlin')",
            @"SELECT * FROM Employees e WHERE e.EmployeeID BETWEEN 1 AND 5 AND e.City LIKE 'L%'",
            @"SELECT * FROM Employees e WHERE e.City SOUNDS LIKE 'London' AND NOT e.City SIMILAR TO 'x'",
            @"SELECT CAST(e.EmployeeID AS STRING), COALESCE(e.City, 'x'), NULLIF(e.City, 'y') FROM Employees e",
            @"SELECT -e.EmployeeID, +1, ~2, 1 + 2 * 3 / 4 % 5 FROM Employees e",
            @"SELECT * FROM Employees e WHERE e.ReportsTo IS NOT NULL",
            @"SELECT e.City FROM Employees e WHERE e.EmployeeID = @p",
            @"SELECT [First Name] FROM [Employees] [e]",
            @"-- leading" + "\n" + @"SELECT 1 -- trailing" + "\n" + @"/* block */ FROM Employees",
            @"SELECT 1" + "\n\n\n" + @"FROM Employees",
            @"SELECT     1     ,     2     FROM     Employees",
        ];
    }
}
