using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class NamedTableReferenceQuickInfoProviderAliasTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new NamedTableReferenceQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single();
        var span = syntax.Alias!.Span;
        var symbol = semanticModel.GetDeclaredSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.TableInstance, markup);
    }

    [Fact]
    public void NamedTableReferenceQuickInfoProvider_MatchesInAlias()
    {
        var query = """
            SELECT  *
            FROM    Employees {e}
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void NamedTableReferenceQuickInfoProvider_DoesNotMatchInAliasUnresolved()
    {
        var query = """
            SELECT  *
            FROM    Xxxxxxxxx {e}
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void NamedTableReferenceQuickInfoProvider_DoesNotMatchInAs()
    {
        var query = """
            SELECT  *
            FROM    Employees {AS} e
            """;

        AssertIsNotMatch(query);
    }
}
