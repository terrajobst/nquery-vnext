using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

public class StackedLayoutTests : FormattingTests
{
    [Fact]
    public void Stacked_PutsTheSelectListBelowTheKeyword()
    {
        var query = "select e.FirstName, e.City from Employees e where e.City = 'London'";

        var expected = """
            SELECT
                e.FirstName,
                e.City
            FROM Employees e
            WHERE e.City = 'London'
            """;

        AssertFormats(query, expected, FormattingOptions.Stacked);
    }

    [Fact]
    public void Stacked_KeepsASingleColumnOnTheKeywordLine()
    {
        AssertFormats("SELECT * FROM Employees", """
            SELECT *
            FROM Employees
            """, FormattingOptions.Stacked);
    }

    [Fact]
    public void Stacked_PutsJoinsAtTheFromLevel()
    {
        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID";

        var expected = """
            SELECT *
            FROM Employees e
            INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            """;

        AssertFormats(query, expected, FormattingOptions.Stacked);
    }

    [Fact]
    public void Compact_KeepsTheSelectListInlineUntilItDoesNotFit()
    {
        var query = "SELECT e.FirstName, e.City FROM Employees e";

        var expected = """
            SELECT e.FirstName, e.City
            FROM Employees e
            """;

        AssertFormats(query, expected, FormattingOptions.Compact);
    }

    [Fact]
    public void Compact_BreaksTheSelectListWhenItDoesNotFit()
    {
        var query = "SELECT e.FirstName, e.LastName, e.City FROM Employees e";

        var expected = """
            SELECT
                e.FirstName,
                e.LastName,
                e.City
            FROM Employees e
            """;

        AssertFormats(query, expected, FormattingOptions.Compact with { MaxLineLength = 30 });
    }
}
