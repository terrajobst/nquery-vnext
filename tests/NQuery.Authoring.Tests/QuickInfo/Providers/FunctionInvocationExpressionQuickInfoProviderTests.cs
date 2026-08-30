using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class FunctionInvocationExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new FunctionInvocationExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<FunctionInvocationExpressionSyntax>().Last();
        var span = syntax.IdentifierToken.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Function, markup);
    }

    [Fact]
    public void FunctionInvocationExpressionQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  LEFT(e.FirstName, {LEN}(e.FirstName))
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void FunctionInvocationExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  LEFT(e.FirstName, {XXX}(e.FirstName))
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void FunctionInvocationExpressionQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  LEFT(e.FirstName, LEN({e.FirstName)})
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
