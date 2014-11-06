using System.Linq;

using Xunit;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
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

        [Fact]
        public void NameExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  {FirstName}
                FROM    Employees
             ";

            AssertIsMatch(query);
        }

        [Fact]
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