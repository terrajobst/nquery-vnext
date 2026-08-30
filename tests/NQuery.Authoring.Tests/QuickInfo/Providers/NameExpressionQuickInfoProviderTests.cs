using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class NameExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new NameExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<NameExpressionSyntax>().Single();
        var span = syntax.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Column, markup);
    }

    [Fact]
    public void NameExpressionQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  {FirstName}
            FROM    Employees
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void NameExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  {Xxx}
            FROM    Employees
            """;

        AssertIsNotMatch(query);
    }
}
