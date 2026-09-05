using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

public class TabularLayoutTests : FormattingTests
{
    [Fact]
    public void Tabular_PadsClauseKeywords()
    {
        var query = "select e.FirstName, e.City from Employees e where e.City = 'London'";

        var expected = """
            SELECT  e.FirstName,
                    e.City
            FROM    Employees e
            WHERE   e.City = 'London'
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_SplitsTwoWordKeywordsAcrossThePad()
    {
        var query = "SELECT e.Country, COUNT(*) FROM Employees e GROUP BY e.Country ORDER BY 1";

        var expected = """
            SELECT  e.Country,
                    COUNT(*)
            FROM    Employees e
            GROUP   BY e.Country
            ORDER   BY 1
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_KeepsSingleColumnOnKeywordLine()
    {
        AssertFormats("SELECT * FROM Employees", """
            SELECT  *
            FROM    Employees
            """);
    }

    [Fact]
    public void Tabular_IndentsJoinsPastThePayload()
    {
        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID";

        var expected = """
            SELECT  *
            FROM    Employees e
                        INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_FlattensJoinChains()
    {
        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID LEFT JOIN Territories t ON t.TerritoryID = et.TerritoryID";

        var expected = """
            SELECT  *
            FROM    Employees e
                        INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
                        LEFT JOIN Territories t ON t.TerritoryID = et.TerritoryID
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_IndentsCommonTableExpressionBodies()
    {
        var query = "WITH Emps AS (SELECT * FROM Employees e WHERE e.ReportsTo IS NULL) SELECT * FROM Emps";

        var expected = """
            WITH Emps AS (
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL
            )
            SELECT  *
            FROM    Emps
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_IndentsDerivedTables()
    {
        var query = "SELECT * FROM (SELECT e.City FROM Employees e WHERE e.EmployeeID > 4) AS D";

        var expected = """
            SELECT  *
            FROM    (
                        SELECT  e.City
                        FROM    Employees e
                        WHERE   e.EmployeeID > 4
                    ) AS D
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_AlignsUnionParts()
    {
        var query = "SELECT 1 FROM Employees UNION ALL SELECT 2 FROM Employees";

        var expected = """
            SELECT  1
            FROM    Employees
            UNION ALL
            SELECT  2
            FROM    Employees
            """;

        AssertFormats(query, expected);
    }

    [Fact]
    public void Tabular_KeepsModifiersOnTheKeywordLine()
    {
        AssertFormats("SELECT DISTINCT TOP 10 e.City FROM Employees e", """
            SELECT  DISTINCT TOP 10 e.City
            FROM    Employees e
            """);
    }

    [Fact]
    public void Tabular_FallsBackToASpaceWhenThePadIsPassed()
    {
        // HAVING is one character short of the pad column; a keyword at or past it still needs a
        // separator.
        AssertFormats("SELECT COUNT(*) FROM Employees e GROUP BY e.City HAVING COUNT(*) > 1", """
            SELECT  COUNT(*)
            FROM    Employees e
            GROUP   BY e.City
            HAVING  COUNT(*) > 1
            """);
    }
}
