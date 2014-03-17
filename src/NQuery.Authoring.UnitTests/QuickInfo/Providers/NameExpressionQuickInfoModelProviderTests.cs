using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class NameExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NameExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NameExpressionSyntax>().Single();
            var span = syntax.Span;
            var symbol = semanticModel.GetSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Column, markup);
        }

        [TestMethod]
        public void NameExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  {FirstName}
                FROM    Employees
             ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void NameExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  {Xxx}
                FROM    Employees
            ";

            AssertIsNotMatch(query);
        }
    }
}