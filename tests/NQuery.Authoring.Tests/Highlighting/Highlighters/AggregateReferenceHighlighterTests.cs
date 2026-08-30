using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters;

public class AggregateReferenceHighlighterTests : HighlighterTests
{
    protected override IHighlighter CreateHighlighter()
    {
        return new SymbolReferenceHighlighter(new SymbolSearchService());
    }

    [Fact]
    public void AggregateReferenceHighlighter_Matches()
    {
        var query = """
            SELECT  {COUNT}(*),
                    {COUNT}(e.ReportsTo)
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }
}
