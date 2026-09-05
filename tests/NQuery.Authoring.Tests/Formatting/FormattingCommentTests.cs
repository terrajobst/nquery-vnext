using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

public class FormattingCommentTests : FormattingTests
{
    [Fact]
    public void Comments_StayTrailingWhenTheyWereTrailing()
    {
        AssertFormats("SELECT 1 -- why\nFROM Employees", """
            SELECT  1 -- why
            FROM    Employees
            """);
    }

    [Fact]
    public void Comments_KeepTheirOwnLineAndTakeTheFollowingIndent()
    {
        AssertFormats("SELECT 1\n-- about the source\nFROM Employees", """
            SELECT  1
            -- about the source
            FROM    Employees
            """);
    }

    [Fact]
    public void Comments_OnTheirOwnLineAreIndentedWithWhatFollows()
    {
        AssertFormats("SELECT e.City,\n-- the second one\ne.Country FROM Employees e", """
            SELECT  e.City,
                    -- the second one
                    e.Country
            FROM    Employees e
            """);
    }

    [Fact]
    public void Comments_InlineBlockCommentsStayInline()
    {
        AssertFormats("SELECT 1 /* one */ + 2 FROM Employees", """
            SELECT  1 /* one */ + 2
            FROM    Employees
            """);
    }

    [Fact]
    public void Comments_AtTheStartOfTheDocumentKeepTheirPosition()
    {
        AssertFormats("/* header */\nSELECT 1 FROM Employees", """
            /* header */
            SELECT  1
            FROM    Employees
            """);
    }

    [Fact]
    public void Comments_ASingleLineCommentAlwaysEndsItsLine()
    {
        // Joining these would turn the FROM clause into part of the comment.
        AssertFormats("SELECT 1 --x\n   FROM Employees", "SELECT  1 --x\nFROM    Employees");
    }
}
