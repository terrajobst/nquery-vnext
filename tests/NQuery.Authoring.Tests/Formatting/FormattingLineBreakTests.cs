using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

// Every line break in the document ends up as NewLine, including the two kinds the formatter
// doesn't lay out itself: inside a string literal and inside a multi line comment. Leaving either
// behind is what would give a converted file mixed endings.
public class FormattingLineBreakTests
{
    [Fact]
    public void Formatting_ConvertsTheBreaksBetweenTokens()
    {
        var query = "SELECT 1,\r\n2\r\nFROM Employees";

        AssertFormats(query, "SELECT  1,\n        2\nFROM    Employees\n", "\n");
    }

    [Fact]
    public void Formatting_ConvertsTheBreaksInsideAMultiLineComment()
    {
        var query = "SELECT 1 /* one\r\n   two */\r\nFROM Employees";

        AssertFormats(query, "SELECT  1 /* one\n   two */\nFROM    Employees\n", "\n");
    }

    [Fact]
    public void Formatting_ConvertsTheBreaksInsideAStringLiteral()
    {
        // This changes the value the query computes, which is the same thing that happens when the
        // file is checked out on a platform with different endings.
        var query = "SELECT 'one\r\ntwo'";

        AssertFormats(query, "SELECT  'one\ntwo'\n", "\n");
    }

    [Fact]
    public void Formatting_ConvertsToCrLfAsWell()
    {
        var query = "SELECT 'one\ntwo' /* three\nfour */";

        AssertFormats(query, "SELECT  'one\r\ntwo' /* three\r\nfour */\r\n", "\r\n");
    }

    [Fact]
    public void Formatting_ConvertsALoneCarriageReturn()
    {
        var query = "SELECT 'one\rtwo'\rFROM Employees";

        AssertFormats(query, "SELECT  'one\ntwo'\nFROM    Employees\n", "\n");
    }

    [Fact]
    public void Formatting_LeavesTheIndentationInsideACommentAlone()
    {
        // The break is ours; what the author lined up after it isn't.
        var query = "SELECT 1 /* one\n      two\n        three */";

        AssertFormats(query, "SELECT  1 /* one\n      two\n        three */\n", "\n");
    }

    private static void AssertFormats(string query, string expected, string newLine)
    {
        var document = DocumentFactory.CreateQuery(query);
        var service = document.Services.GetService<FormattingService>();
        var options = FormattingOptions.Default with { NewLine = newLine };

        var formatted = service.Format(document, options);
        Assert.Equal(expected, formatted.Text.GetText());

        // Formatting formatted text has to be a no-op, which is where a half-converted break would
        // show up as a change that never settles.
        var again = service.Format(formatted, options);
        Assert.Equal(expected, again.Text.GetText());
    }
}
