using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class PropertyAccessExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new PropertyAccessExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<PropertyAccessExpressionSyntax>().Single();
        var span = syntax.IdentifierToken.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Property, markup);
    }

    [Fact]
    public void PropertyAccessExpressionQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  FirstName.{Length}
            FROM    Employees
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void PropertyAccessExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  FirstName.{Xxx}
            FROM    Employees
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void PropertyAccessExpressionQuickInfoProvider_DoesNotMatchBeforeDot()
    {
        var query = """
            SELECT  {FirstName}.Length
            FROM    Employees
            """;

        AssertIsNotMatch(query);
    }
}
