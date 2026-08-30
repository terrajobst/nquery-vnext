using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class JoinCompletionProviderTests
{
    private static void AssertIsMatch(string queryWithJoinMarker)
    {
        var queryWithJoin = queryWithJoinMarker.ParseSingleSpan(out var span);
        var condition = queryWithJoin.Substring(span);
        var query = queryWithJoin.Remove(span.Start, span.Length);
        var position = span.Start;

        var services = DocumentFactory.ServicesWithOnly<ICompletionProvider>(new JoinCompletionProvider());
        var document = DocumentFactory.CreateQuery(query, services);

        var completionResult = document.Services.GetService<CompletionService>()
                                      .GetResult(DocumentView.Create(document, position));
        var item = completionResult.Items.Single(i => i.InsertionText == condition);

        Assert.Equal(Glyph.Relation, item.Glyph);
        Assert.Equal(condition, item.Description);
        Assert.Equal(condition, item.DisplayText);
        Assert.Equal(condition, item.InsertionText);
        Assert.Null(item.Symbol);
    }

    [Fact]
    public void JoinCompletionProvider_ReturnsJoin()
    {
        var query = """
            SELECT  *
            FROM    Employees e
                        INNER JOIN EmployeeTerritories et ON {et.EmployeeID = e.EmployeeID}
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void JoinCompletionProvider_CorrectlyEscapes()
    {
        var query = """
            SELECT  *
            FROM    Orders o
                        INNER JOIN [Order Details] ON {[Order Details].OrderID = o.OrderID}
            """;

        AssertIsMatch(query);
    }
}
