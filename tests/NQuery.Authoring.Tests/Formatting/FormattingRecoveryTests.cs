using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

// Most of the time an editor asks for formatting the document doesn't parse, so a broken region has
// to cost the formatting of that region and nothing more.
public class FormattingRecoveryTests : FormattingTests
{
    [Theory]
    [InlineData(@"")]
    [InlineData(@"   ")]
    [InlineData(@"SELECT")]
    [InlineData(@"SELECT FROM")]
    [InlineData(@"SELECT 1 FROM")]
    [InlineData(@"SELECT , FROM Employees")]
    [InlineData(@"SELECT * FROM Employees e INNER JOIN")]
    [InlineData(@"SELECT * FROM Employees e WHERE")]
    [InlineData(@"SELECT ((((")]
    [InlineData(@"WITH X AS (SELECT")]
    [InlineData(@"@@@")]
    [InlineData(@"SELECT 'unterminated")]
    public void Formatting_SurvivesBrokenInput(string query)
    {
        var document = DocumentFactory.CreateQuery(query);
        var service = document.Services.GetService<FormattingService>();
        var formatted = service.Format(document, GetOptions(null));

        var before = document.GetSyntaxTree().Root;
        var after = formatted.GetSyntaxTree().Root;

        Assert.True(before.IsEquivalentTo(after), $"Not equivalent:\n{formatted.Text.GetText()}");

        var again = service.Format(formatted, GetOptions(null));
        Assert.Equal(formatted.Text.GetText(), again.Text.GetText());
    }

    [Fact]
    public void Formatting_LeavesABrokenRegionAloneAndFormatsTheRest()
    {
        // The WHERE clause has no predicate, so the text around the token the parser had to invent is
        // copied through -- and everything above it is still formatted.
        var document = DocumentFactory.CreateQuery(@"select 1 from Employees where");
        var service = document.Services.GetService<FormattingService>();
        var formatted = service.Format(document, GetOptions(null));

        Assert.Equal("SELECT  1\nFROM    Employees\nWHERE", formatted.Text.GetText());
    }
}
