using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class CoalesceExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new CoalesceExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<CoalesceExpressionSyntax>().Single();
        var span = syntax.CoalesceKeyword.Span;
        var markup = SymbolMarkup.ForCoalesceSymbol();
        return new QuickInfoResult(semanticModel, span, Glyph.Function, markup);
    }

    [Fact]
    public void CoalesceExpressionQuickInfoProvider_MatchesInCoalesce()
    {
        var query = """
            SELECT  {COALESCE}(e.ReportsTo, e.EmployeeId)
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void CoalesceExpressionQuickInfoProvider_DoesNotMatchInParentheses()
    {
        var query = """
            SELECT  COALESCE({e.ReportsTo, e.EmployeeId)}
            FROM    Employees e
            """;

        AssertIsNotMatch(query);
    }
}
