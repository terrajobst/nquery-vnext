using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
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
            return new QuickInfoModel(semanticModel, span, Glyph.Column, markup);
        }

        [Fact]
        public void ExpressionSelectColumnQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS {[Full Name]}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [Fact]
        public void ExpressionSelectColumnQuickInfoModelProvider_DoesNotMatchInExpression()
        {
            var query = @"
                SELECT  {e.FirstName + ' ' + e.LastName} AS [Full Name]
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void ExpressionSelectColumnQuickInfoModelProvider_DoesNotMatchForAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName {AS} [Full Name]
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
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