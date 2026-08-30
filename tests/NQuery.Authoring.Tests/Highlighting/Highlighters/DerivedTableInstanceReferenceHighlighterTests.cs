using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters;

public class DerivedTableInstanceReferenceHighlighterTests : HighlighterTests
{
    protected override IHighlighter CreateHighlighter()
    {
        return new SymbolReferenceHighlighter(new SymbolSearchService());
    }

    [Fact]
    public void DerivedTableInstanceReferenceHighlighter_Matches()
    {
        var query = """
            SELECT  {em}.FirstName,
                    {em}.LastName
            FROM    (SELECT * FROM Employees) {em}
            """;

        AssertIsMatch(query);
    }
}
