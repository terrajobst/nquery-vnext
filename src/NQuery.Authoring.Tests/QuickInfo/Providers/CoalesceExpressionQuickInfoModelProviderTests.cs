using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
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

        [Fact]
        public void CoalesceExpressionQuickInfoModelProvider_MatchesInCoalesce()
        {
            var query = @"
                SELECT  {COALESCE}(e.ReportsTo, e.EmployeeId)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [Fact]
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