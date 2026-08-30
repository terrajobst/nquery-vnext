using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class NamedTableReferenceQuickInfoProviderTableTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new NamedTableReferenceQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single();
        var span = syntax.IdentifierToken.Span;
        var symbol = semanticModel.GetDeclaredSymbol(syntax)!.Table;
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Table, markup);
    }

    [Fact]
    public void NamedTableReferenceQuickInfoProvider_MatchesInTable()
    {
        var query = """
            SELECT  *
            FROM    {Employees} e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void NamedTableReferenceQuickInfoProvider_DoesNotMatchUnresolved()
    {
        var query = """
            SELECT  *
            FROM    {Xxxxxxxxx} e
            """;

        AssertIsNotMatch(query);
    }
}
