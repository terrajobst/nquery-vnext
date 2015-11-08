using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class WildcardSelectColumnQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new WildcardSelectColumnQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<WildcardSelectColumnSyntax>().Single();
            var span = syntax.TableName.Span;
            var symbol = semanticModel.GetTableInstance(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, Glyph.TableInstance, markup);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_MatchesInAlias()
        {
            var query = @"
                SELECT  {e}.*
                FROM    Employees e
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_DoesNotMatchesUnresolved()
        {
            var query = @"
                SELECT  {x}.*
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [Fact]
        public void WildcardSelectColumnQuickInfoModelProvider_DoesNotMatchAfterDot()
        {
            var query = @"
                SELECT  e.{*}
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }
    }
}