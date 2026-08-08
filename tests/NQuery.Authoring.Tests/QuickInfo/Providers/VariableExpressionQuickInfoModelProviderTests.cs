using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.Metadata;

namespace NQuery.Authoring.Tests.QuickInfo.Providers;

public class VariableExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
{
    protected override IQuickInfoModelProvider CreateProvider()
    {
        return new VariableExpressionQuickInfoModelProvider();
    }

    protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var syntax = syntaxTree.Root.DescendantNodes().OfType<VariableExpressionSyntax>().Single();
        var span = syntax.Span;
        var symbol = semanticModel.GetSymbol(syntax);
        var markup = SymbolMarkup.ForSymbol(symbol!);
        return new QuickInfoModel(semanticModel, span, Glyph.Variable, markup);
    }

    [Fact]
    public void VariableExpressionQuickInfoModelProvider_MatchesInName()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = @{EmployeeId}
            """;

        AssertIsMatch(query, dc => dc.AddVariables(VariableDefinition.Create("EmployeeId", typeof(int))));
    }

    [Fact]
    public void VariableExpressionQuickInfoModelProvider_MatchesInAt()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = {@}EmployeeId
            """;

        AssertIsMatch(query, dc => dc.AddVariables(VariableDefinition.Create("EmployeeId", typeof(int))));
    }

    [Fact]
    public void VariableExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
    {
        var query = """
            SELECT  *
            FROM    Employees e
            WHERE   e.EmployeeId = {@EmployeeId}
            """;

        AssertIsNotMatch(query);
    }
}
