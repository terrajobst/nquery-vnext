using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class MethodInvocationExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new MethodInvocationExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<MethodInvocationExpressionSyntax>().Last();
            var span = syntax.Name.Span;
            var symbol = semanticModel.GetSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, Glyph.Method, markup);
        }

        [Fact]
        public void MethodInvocationExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  e.EmployeeID.ToString(e.FirstName.{Substring}(2))
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [Fact]
        public void MethodInvocationExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  e.EmployeeID.ToString(e.FirstName.{Xxx}(2))
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void MethodInvocationExpressionQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  e.EmployeeID.ToString(e.FirstName.Substring({2)})
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}