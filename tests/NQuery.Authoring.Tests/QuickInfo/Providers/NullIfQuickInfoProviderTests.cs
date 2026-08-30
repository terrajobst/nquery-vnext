using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class NullIfQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new NullIfQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<NullIfExpressionSyntax>().Single();
        var span = syntax.NullIfKeyword.Span;
        var markup = SymbolMarkup.ForNullIfSymbol();
        return new QuickInfoResult(semanticModel, span, Glyph.Function, markup);
    }

    [Fact]
    public void NullIfQuickInfoProvider_MatchesInNullIf()
    {
        var query = """
            SELECT  {NULLIF}(e.FirstName, 'Andrew')
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void NullIfQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  NULLIF({e.FirstName, 'Andrew')}
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
