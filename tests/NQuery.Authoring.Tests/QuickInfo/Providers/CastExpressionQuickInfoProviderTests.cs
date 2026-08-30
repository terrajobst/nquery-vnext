using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class CastExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new CastExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<CastExpressionSyntax>().Single();
        var span = syntax.CastKeyword.Span;
        var markup = SymbolMarkup.ForCastSymbol();
        return new QuickInfoResult(semanticModel, span, Glyph.Function, markup);
    }

    [Fact]
    public void CastExpressionQuickInfoProvider_MatchesInCast()
    {
        var query = """
            SELECT  {CAST}(1 AS FLOAT)
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void CastExpressionQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  CAST({1 AS FLOAT)}
            """;

        AssertIsNotMatch(query);
    }
}
