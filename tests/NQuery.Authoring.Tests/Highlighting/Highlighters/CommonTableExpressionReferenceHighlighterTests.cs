using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters;

public class CommonTableExpressionReferenceHighlighterTests : HighlighterTests
{
    protected override IHighlighter CreateHighlighter()
    {
        return new SymbolReferenceHighlighter(new SymbolSearchService());
    }

    [Fact]
    public void CommonTableExpressionReferenceHighlighter_Matches()
    {
        var query = """
            WITH {Emps} (
                SELECT  *
                FROM    Employees
            )
            SELECT  *
            FROM    {Emps} em
            """;

        AssertIsMatch(query);
    }
}
