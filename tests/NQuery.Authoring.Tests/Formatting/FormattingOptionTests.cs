using NQuery.Authoring.Formatting;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Formatting;

public class FormattingOptionTests : FormattingTests
{
    [Fact]
    public void Keywords_AreUpperCasedByDefault()
    {
        AssertFormats("select 1 from Employees", """
            SELECT  1
            FROM    Employees
            """);
    }

    [Fact]
    public void Keywords_CanBeLowerCased()
    {
        var options = FormattingOptions.Tabular with { Keywords = Casing.Lower };

        AssertFormats("SELECT 1 FROM Employees", """
            select  1
            from    Employees
            """, options);
    }

    [Fact]
    public void Keywords_CanBeLeftAlone()
    {
        var options = FormattingOptions.Tabular with { Keywords = Casing.Preserve };

        AssertFormats("Select 1 From Employees", """
            Select  1
            From    Employees
            """, options);
    }

    [Fact]
    public void Keywords_DoNotTouchNamesSpelledLikeKeywords()
    {
        // The parser rewrote the kind of the contextual keywords it actually used as keywords, so a
        // bracketed name that reads like one is still an identifier here.
        AssertFormats("SELECT [Select] FROM Employees", """
            SELECT  [Select]
            FROM    Employees
            """);
    }

    [Fact]
    public void Identifiers_ArePreservedByDefault()
    {
        AssertFormats("SELECT [e].[City] FROM Employees [e]", """
            SELECT  [e].[City]
            FROM    Employees [e]
            """);
    }

    [Fact]
    public void Identifiers_CanBeUnquotedWhenRedundant()
    {
        var options = FormattingOptions.Tabular with { Identifiers = IdentifierQuoting.WhenRequired };

        AssertFormats("SELECT [e].\"City\" FROM Employees [e]", """
            SELECT  e.City
            FROM    Employees e
            """, options);
    }

    [Fact]
    public void Identifiers_KeepTheirQuotingWhenItIsLoadBearing()
    {
        var options = FormattingOptions.Tabular with { Identifiers = IdentifierQuoting.WhenRequired };

        // A name with a space needs its brackets; one that would lex as a contextual keyword would
        // change what the parser does with it, which is a rename rather than a formatting change.
        AssertFormats("SELECT [First Name], [Select] FROM Employees", """
            SELECT  [First Name],
                    [Select]
            FROM    Employees
            """, options);
    }

    [Fact]
    public void On_CanBeGivenItsOwnLine()
    {
        var options = FormattingOptions.Tabular with { On = OnPlacement.OwnLine };

        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID";

        var expected = """
            SELECT  *
            FROM    Employees e
                        INNER JOIN EmployeeTerritories et
                            ON et.EmployeeID = e.EmployeeID
            """;

        AssertFormats(query, expected, options);
    }

    [Fact]
    public void On_CanBreakOnlyWhenTheConditionIsCompound()
    {
        var options = FormattingOptions.Tabular with { On = OnPlacement.OwnLineWhenMultiple };

        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID AND et.TerritoryID > 0";

        var expected = """
            SELECT  *
            FROM    Employees e
                        INNER JOIN EmployeeTerritories et
                            ON et.EmployeeID = e.EmployeeID AND et.TerritoryID > 0
            """;

        AssertFormats(query, expected, options);
    }

    [Fact]
    public void Joins_CanSitAtTheFromLevel()
    {
        var options = FormattingOptions.Tabular with { Joins = JoinIndentation.AtFromLevel };

        var query = "SELECT * FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID";

        var expected = """
            SELECT  *
            FROM    Employees e
                    INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            """;

        AssertFormats(query, expected, options);
    }

    [Fact]
    public void MaxLineLength_BreaksLogicalChainsWithLeadingOperators()
    {
        var options = FormattingOptions.Tabular with { MaxLineLength = 40 };

        var query = "SELECT * FROM Employees e WHERE e.City = 'London' AND e.Country = 'UK'";

        var expected = """
            SELECT  *
            FROM    Employees e
            WHERE   e.City = 'London'
                    AND e.Country = 'UK'
            """;

        AssertFormats(query, expected, options);
    }

    // Zero turns off wrapping, and wrapping is about width. A break the layout asks for whatever the
    // width -- a clause keyword, a CASE label -- is structure, and still happens.
    [Fact]
    public void MaxLineLength_OfZeroOnlyStopsBreakingForWidth()
    {
        var options = FormattingOptions.Tabular with { MaxLineLength = 0 };

        var query = "SELECT CASE WHEN e.City = 'London' THEN 1 ELSE 2 END FROM Employees e WHERE e.City = 'London' AND e.Country = 'UK' AND e.EmployeeID > 4";

        var expected = """
            SELECT  CASE
                        WHEN e.City = 'London' THEN 1
                        ELSE 2
                    END
            FROM    Employees e
            WHERE   e.City = 'London' AND e.Country = 'UK' AND e.EmployeeID > 4
            """;

        AssertFormats(query, expected, options);
    }

    [Fact]
    public void MaxLineLength_BreaksArithmeticWithLeadingOperators()
    {
        var options = FormattingOptions.Tabular with { MaxLineLength = 40 };

        var query = "SELECT od.UnitPrice * od.Quantity * (1 - od.Discount) + od.Freight AS Total FROM Order_Details od";

        var expected = """
            SELECT  od.UnitPrice * od.Quantity
                    * (1 - od.Discount)
                    + od.Freight AS Total
            FROM    Order_Details od
            """;

        AssertFormats(query, expected, options);
    }

    // The operators break outermost first because their groups nest the way precedence does, so what
    // binds tighter than the operator that gave way stays on the line with its operands.
    [Fact]
    public void MaxLineLength_BreaksComparisonsBeforeWhatBindsTighter()
    {
        var options = FormattingOptions.Tabular with { MaxLineLength = 40 };

        var query = "SELECT 1 FROM Employees e WHERE e.ReportsTo + e.EmployeeID * 2 > e.Salary - 100000";

        var expected = """
            SELECT  1
            FROM    Employees e
            WHERE   e.ReportsTo + e.EmployeeID * 2
                    > e.Salary - 100000
            """;

        AssertFormats(query, expected, options);
    }

    [Fact]
    public void IndentSize_IsHonored()
    {
        var options = FormattingOptions.Stacked with { IndentSize = 2 };

        AssertFormats("SELECT e.City, e.Country FROM Employees e", """
            SELECT
              e.City,
              e.Country
            FROM Employees e
            """, options);
    }

    [Fact]
    public void UseTabs_IndentsWithTabs()
    {
        var options = FormattingOptions.Stacked with { UseTabs = true };

        AssertFormats("SELECT e.City, e.Country FROM Employees e", "SELECT\n\te.City,\n\te.Country\nFROM Employees e", options);
    }

    [Fact]
    public void MaxBlankLines_KeepsOneByDefault()
    {
        AssertFormats("SELECT 1\n\n\n\nFROM Employees", "SELECT  1\n\nFROM    Employees");
    }

    [Fact]
    public void MaxBlankLines_OfZeroCollapsesThem()
    {
        var options = FormattingOptions.Tabular with { MaxBlankLines = 0 };

        AssertFormats("SELECT 1\n\n\nFROM Employees", "SELECT  1\nFROM    Employees", options);
    }

    [Fact]
    public void InsertFinalNewline_CanBeTurnedOff()
    {
        var options = GetOptions(FormattingOptions.Tabular with { InsertFinalNewline = false });

        var document = DocumentFactory.CreateQuery("SELECT 1 FROM Employees");
        var service = document.Services.GetService<FormattingService>();
        var formatted = service.Format(document, options);

        Assert.Equal("SELECT  1\nFROM    Employees", formatted.Text.GetText());
    }

    [Fact]
    public void Expressions_DoNotGetAFinalNewline()
    {
        var document = DocumentFactory.CreateExpression("1+2*  3");
        var service = document.Services.GetService<FormattingService>();
        var formatted = service.Format(document, GetOptions(null));

        Assert.Equal("1 + 2 * 3", formatted.Text.GetText());
    }

    [Fact]
    public void Range_OnlyChangesWhatItReaches()
    {
        var query = "SELECT    1,2 FROM     Employees WHERE 1=1";

        var document = DocumentFactory.CreateQuery(query);
        var service = document.Services.GetService<FormattingService>();
        var span = TextSpan.FromBounds(query.IndexOf("FROM"), query.IndexOf("WHERE"));
        var formatted = service.Format(document, span, GetOptions(null));

        Assert.Equal("SELECT    1,2\nFROM    Employees\nWHERE 1=1", formatted.Text.GetText());
    }
}
