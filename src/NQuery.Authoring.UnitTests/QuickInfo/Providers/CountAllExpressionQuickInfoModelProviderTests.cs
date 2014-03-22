using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class CountAllExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new CountAllExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<CountAllExpressionSyntax>().Single();
            var span = syntax.Name.Span;
            var symbol = GetCountAggregate(semanticModel.Compilation.DataContext);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Aggregate, markup);
        }

        private static AggregateSymbol GetCountAggregate(DataContext dataContext)
        {
            var aggregates = dataContext.Aggregates;
            return aggregates.Single(a => a.Name == "COUNT");
        }

        [TestMethod]
        public void CountAllExpressionQuickInfoModelProvider_MatchesInCount()
        {
            var query = @"
                SELECT  {COUNT}(*)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CountAllExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  {COUNT}(*)
                FROM    Employees e
            ";

            AssertIsNotMatch(query, dc => dc.RemoveAggregates(GetCountAggregate(dc)));
        }

        [TestMethod]
        public void CountAllExpressionQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  COUNT({*)}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}