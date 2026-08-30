using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters;

public class TableInstanceReferenceHighlighterTests : HighlighterTests
{
    protected override IHighlighter CreateHighlighter()
    {
        return new SymbolReferenceHighlighter(new SymbolSearchService());
    }

    [Fact]
    public void TableInstanceReferenceHighlighter_Matches()
    {
        var query = """
            SELECT  {em}.FirstName,
                    {em}.LastName
            FROM    Employees {em}
            """;

        AssertIsMatch(query);
    }
}
