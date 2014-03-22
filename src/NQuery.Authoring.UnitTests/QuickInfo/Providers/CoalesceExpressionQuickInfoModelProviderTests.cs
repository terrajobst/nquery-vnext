using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class CoalesceExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new CoalesceExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<CoalesceExpressionSyntax>().Single();
            var span = syntax.CoalesceKeyword.Span;
            var markup = SymbolMarkup.ForCoalesceSymbol();
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Function, markup);
        }

        [TestMethod]
        public void CoalesceExpressionQuickInfoModelProvider_MatchesInCoalesce()
        {
            var query = @"
                SELECT  {COALESCE}(e.ReportsTo, e.EmployeeId)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CoalesceExpressionQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  COALESCE({e.ReportsTo, e.EmployeeId)}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}