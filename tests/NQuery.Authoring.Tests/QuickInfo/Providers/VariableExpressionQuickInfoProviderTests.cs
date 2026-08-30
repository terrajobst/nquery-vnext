using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.Metadata;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class VariableExpressionQuickInfoProviderTests : QuickInfoProviderTests
{
    protected override IQuickInfoProvider CreateProvider()
    {
        return new VariableExpressionQuickInfoProvider();
    }

    protected override QuickInfoResult CreateExpectedResult(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<VariableExpressionSyntax>().Single();
        var span = syntax.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoResult(semanticModel, span, Glyph.Variable, markup);
    }

    [Fact]
    public void VariableExpressionQuickInfoProvider_MatchesInName()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = @{EmployeeId}
            """;

        AssertIsMatch(query, dc => dc.AddVariables(VariableDefinition.Create("EmployeeId", typeof(int))));
    }

    [Fact]
    public void VariableExpressionQuickInfoProvider_MatchesInAt()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = {@}EmployeeId
            """;

        AssertIsMatch(query, dc => dc.AddVariables(VariableDefinition.Create("EmployeeId", typeof(int))));
    }

    [Fact]
    public void VariableExpressionQuickInfoProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = {@EmployeeId}
            """;

        AssertIsNotMatch(query);
    }
}
