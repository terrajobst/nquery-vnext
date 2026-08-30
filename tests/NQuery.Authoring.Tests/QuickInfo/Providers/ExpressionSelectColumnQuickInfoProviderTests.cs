using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class ExpressionSelectColumnQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new ExpressionSelectColumnQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<ExpressionSelectColumnSyntax>().Single();
        var span = syntax.Alias!.IdentifierToken.Span;
        var symbol = semanticModel.GetDeclaredSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Column, markup);
    }

    [Fact]
    public void ExpressionSelectColumnQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  e.FirstName + ' ' + e.LastName AS {[Full Name]}
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void ExpressionSelectColumnQuickInfoProvider_DoesNotMatchInExpression()
    {
        var query = """
            SELECT  {e.FirstName + ' ' + e.LastName} AS [Full Name]
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void ExpressionSelectColumnQuickInfoProvider_DoesNotMatchForAs()
    {
        var query = """
            SELECT  e.FirstName + ' ' + e.LastName {AS} [Full Name]
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }

    [Fact]
    public void ExpressionSelectColumnQuickInfoProvider_DoesNotMatchForUnnamed()
    {
        var query = """
            SELECT  {e.FirstName + ' ' + e.LastName}
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
