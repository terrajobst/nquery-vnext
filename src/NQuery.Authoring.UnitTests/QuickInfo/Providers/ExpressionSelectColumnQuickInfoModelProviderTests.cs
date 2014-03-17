using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class ExpressionSelectColumnQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new ExpressionSelectColumnQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<ExpressionSelectColumnSyntax>().Single();
            var span = syntax.Alias.Identifier.Span;
            var symbol = semanticModel.GetDeclaredSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Column, markup);
        }

        [TestMethod]
        public void ExpressionSelectColumnQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS {[Full Name]}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void ExpressionSelectColumnQuickInfoModelProvider_DoesNotMatchInExpression()
        {
            var query = @"
                SELECT  {e.FirstName + ' ' + e.LastName} AS [Full Name]
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [TestMethod]
        public void ExpressionSelectColumnQuickInfoModelProvider_DoesNotMatchForAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName {AS} [Full Name]
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [TestMethod]
        public void ExpressionSelectColumnQuickInfoModelProvider_DoesNotMatchForUnanmed()
        {
            var query = @"
                SELECT  {e.FirstName + ' ' + e.LastName}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}