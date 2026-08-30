using NQuery.Authoring.Selection;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Selection;

public class SelectionServiceTests
{
    [Fact]
    public void SelectionService_Grows()
    {
        var query = """
            SELECT  e.First|Name
            FROM    Employees e
            """;

        var document = DocumentFactory.CreateQuery(query, out int position);
        var text = document.Text;
        var selection = document.Services.GetService<SelectionService>();

        TextSpan Extend(TextSpan span)
        {
            return selection.ExtendSelection(DocumentView.Create(document, position, span));
        }

        var start = new TextSpan(position, 0);

        var firstTime = Extend(start);
        Assert.Equal("FirstName", text.GetText(firstTime));

        var secondTime = Extend(firstTime);
        Assert.Equal("e.FirstName", text.GetText(secondTime));

        var thirdTime = Extend(secondTime);
        Assert.Equal("SELECT  e.FirstName", text.GetText(thirdTime));

        var fourthTime = Extend(thirdTime);
        Assert.Equal(text.GetText().Trim(), text.GetText(fourthTime));

        var fifthTime = Extend(fourthTime);
        Assert.Equal(text.GetText().TrimStart(), text.GetText(fifthTime));

        var sixthTime = Extend(fifthTime);
        Assert.Equal(fifthTime, sixthTime);
    }

    // ExtendSelection returns the smallest candidate the selection doesn't already cover, which is
    // only the same as "the next one out" while every candidate encloses the one before it. A
    // provider offering a span that merely overlaps would break that silently, so pin the invariant.
    [Fact]
    public void SelectionService_CandidateSpansAlwaysNest()
    {
        string[] queries =
        [
            "SELECT e.FirstName, e.LastName FROM Employees e WHERE e.City = 'London'",
            "WITH X (a, b) AS (SELECT 1, 2) SELECT a, b FROM X ORDER BY a DESC",
            "SELECT COUNT(*), SUM(o.Freight) FROM Orders o GROUP BY o.CustomerID HAVING COUNT(*) > 1",
            "SELECT CAST(e.City AS INT) FROM Employees e INNER JOIN Orders o ON o.EmployeeID = e.EmployeeID",
            "SELECT * FROM (SELECT 1 AS x) d"
        ];

        var providers = DocumentFactory.DefaultServices.GetServices<ISelectionSpanProvider>();

        foreach (var query in queries)
        {
            var document = DocumentFactory.CreateQuery(query);
            var root = document.GetSyntaxTree().Root;

            for (var position = 0; position <= query.Length; position++)
            {
                var view = DocumentView.Create(document, position);
                var candidates = GetEnclosingSpans(root, position)
                                    .Concat(providers.SelectMany(p => p.GetSpans(view, default)))
                                    .ToArray();

                foreach (var a in candidates)
                {
                    foreach (var b in candidates)
                        Assert.True(a.Contains(b) || b.Contains(a), $"{a} and {b} do not nest at {position} in {query}");
                }
            }
        }
    }

    private static IEnumerable<TextSpan> GetEnclosingSpans(SyntaxNode root, int position)
    {
        var token = root.FindToken(position).GetPreviousTokenIfEndOfFile();
        yield return token.Span;

        for (var node = token.Parent; node is not null; node = node.Parent)
            yield return node.Span;
    }
}
