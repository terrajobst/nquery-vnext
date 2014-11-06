using System.Linq;

using Xunit;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    public class FunctionInvocationExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new FunctionInvocationExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<FunctionInvocationExpressionSyntax>().Last();
            var span = syntax.Name.Span;
            var symbol = semanticModel.GetSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Function, markup);
        }

        [Fact]
        public void FunctionInvocationExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  LEFT(e.FirstName, {LEN}(e.FirstName))
                FROM    Employees e
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void FunctionInvocationExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  LEFT(e.FirstName, {XXX}(e.FirstName))
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void FunctionInvocationExpressionQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  LEFT(e.FirstName, LEN({e.FirstName)})
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}