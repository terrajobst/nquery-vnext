using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class MethodInvocationExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new MethodInvocationExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<MethodInvocationExpressionSyntax>().Last();
        var span = syntax.IdentifierToken.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Method, markup);
    }

    [Fact]
    public void MethodInvocationExpressionQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  e.EmployeeID.ToString(e.FirstName.{Substring}(2))
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void MethodInvocationExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  e.EmployeeID.ToString(e.FirstName.{Xxx}(2))
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void MethodInvocationExpressionQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  e.EmployeeID.ToString(e.FirstName.Substring({2)})
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
