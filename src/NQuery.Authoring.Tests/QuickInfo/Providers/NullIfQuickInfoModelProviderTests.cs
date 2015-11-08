using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class NullIfQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NullIfQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NullIfExpressionSyntax>().Single();
            var span = syntax.NullIfKeyword.Span;
            var markup = SymbolMarkup.ForNullIfSymbol();
            return new QuickInfoModel(semanticModel, span, Glyph.Function, markup);
        }

        [Fact]
        public void NullIfQuickInfoModelProvider_MatchesInNullIf()
        {
            var query = @"
                SELECT  {NULLIF}(e.FirstName, 'Andrew')
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [Fact]
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