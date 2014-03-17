using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class MethodInvocationExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new MethodInvocationExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<MethodInvocationExpressionSyntax>().Last();
            var span = syntax.Name.Span;
            var symbol = semanticModel.GetSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Method, markup);
        }

        [TestMethod]
        public void MethodInvocationExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  e.EmployeeID.ToString(e.FirstName.{Substring}(2))
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void MethodInvocationExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  e.EmployeeID.ToString(e.FirstName.{Xxx}(2))
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [TestMethod]
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