using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class WildcardSelectColumnQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new WildcardSelectColumnQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<WildcardSelectColumnSyntax>().Single();
        var span = syntax.IdentifierToken!.Span;
        var symbol = semanticModel.GetTableInstance(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.TableInstance, markup);
    }

    [Fact]
    public void WildcardSelectColumnQuickInfoProvider_MatchesInAlias()
    {
        var query = """
            SELECT  {e}.*
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void WildcardSelectColumnQuickInfoProvider_DoesNotMatchesUnresolved()
    {
        var query = """
            SELECT  {x}.*
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void WildcardSelectColumnQuickInfoProvider_DoesNotMatchAfterDot()
    {
        var query = """
            SELECT  e.{*}
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
