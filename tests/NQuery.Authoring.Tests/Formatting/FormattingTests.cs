using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

public abstract class FormattingTests
{
    // Expected text is written without its final newline, which every test would otherwise have to
    // spell out; InsertFinalNewline has a test of its own.
    protected static void AssertFormats(string query, string expected, FormattingOptions? options = null)
    {
        var document = DocumentFactory.CreateQuery(query);
        var service = document.Services.GetService<FormattingService>();
        var resolved = GetOptions(options);

        var formatted = service.Format(document, resolved);
        Assert.Equal(expected + "\n", formatted.Text.GetText());

        // Every assertion doubles as an idempotence check: formatting formatted text has to be a
        // no-op, and that catches a whole class of rule that only looks right the first time.
        var again = service.Format(formatted, resolved);
        Assert.Equal(expected + "\n", again.Text.GetText());
    }

    protected static FormattingOptions GetOptions(FormattingOptions? options)
    {
        // The tests are written with LF literals, so the newline can't come from the environment.
        return (options ?? FormattingOptions.Default) with { NewLine = "\n" };
    }
}
