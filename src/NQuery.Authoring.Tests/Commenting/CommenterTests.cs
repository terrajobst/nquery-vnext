using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Commenting;

public abstract class CommenterTests
{
    protected abstract SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan span);

    protected void AssertIsMatch(string queryWithMarkers, string expectedQuery)
    {
        var query = queryWithMarkers.ParseSingleSpan(out var selection);

        var syntaxTree = SyntaxTree.ParseQuery(query);
        var actualTree = ToggleComment(syntaxTree, selection);

        var actualQuery = actualTree.Text.GetText();

        Assert.Equal(expectedQuery, actualQuery);
    }
}
