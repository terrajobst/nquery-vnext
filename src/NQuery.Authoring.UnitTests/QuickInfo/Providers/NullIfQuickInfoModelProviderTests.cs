using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class NullIfQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NullIfQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NullIfExpressionSyntax>().Single();
            var span = syntax.NullIfKeyword.Span;
            var markup = SymbolMarkup.ForNullIfSymbol();
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Function, markup);
        }

        [TestMethod]
        public void NullIfQuickInfoModelProvider_MatchesInNullIf()
        {
            var query = @"
                SELECT  {NULLIF}(e.FirstName, 'Andrew')
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void NullIfQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  NULLIF({e.FirstName, 'Andrew')}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}