using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.Metadata;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class CountAllExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new CountAllExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<CountAllExpressionSyntax>().Single();
        var span = syntax.IdentifierToken.Span;
        var symbol = semanticModel.Aggregates.Single(a => a.Name == "COUNT");
        var markup = SymbolMarkup.ForSymbol(symbol);
        return new QuickInfoResult(semanticModel, span, Glyph.Aggregate, markup);
    }

    private static AggregateDefinition GetCountAggregate(Catalog catalog)
    {
        var aggregates = catalog.Aggregates;
        return aggregates.Single(a => a.Name == "COUNT");
    }

    [Fact]
    public void CountAllExpressionQuickInfoProvider_MatchesInCount()
    {
        var query = """
            SELECT  {COUNT}(*)
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void CountAllExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  {COUNT}(*)
            FROM    Employees e
            """;

        AssertIsNotMatch(query, dc => dc.RemoveAggregates(GetCountAggregate(dc)));
    }

    [Fact]
    public void CountAllExpressionQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  COUNT({*)}
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
