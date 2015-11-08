using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class PropertyAccessExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new PropertyAccessExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<PropertyAccessExpressionSyntax>().Single();
            var span = syntax.Name.Span;
            var symbol = semanticModel.GetSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, Glyph.Property, markup);
        }

        [Fact]
        public void PropertyAccessExpressionQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  FirstName.{Length}
                FROM    Employees
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void PropertyAccessExpressionQuickInfoModelProvider_DoesNotMatchForUnresolved()
        {
            var query = @"
                SELECT  FirstName.{Xxx}
                FROM    Employees
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void PropertyAccessExpressionQuickInfoModelProvider_DoesNotMatchBeforeDot()
        {
            var query = @"
                SELECT  {FirstName}.Length
                FROM    Employees
            ";

            AssertIsNotMatch(query);
        }
    }
}