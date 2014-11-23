﻿using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class CastExpressionQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new CastExpressionQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<CastExpressionSyntax>().Single();
            var span = syntax.CastKeyword.Span;
            var markup = SymbolMarkup.ForCastSymbol();
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Function, markup);
        }

        [Fact]
        public void CastExpressionQuickInfoModelProvider_MatchesInCast()
        {
            var query = @"
                SELECT  {CAST}(1 AS FLOAT)
            ";

            AssertIsMatch(query);
        }

        [Fact]
        public void CastExpressionQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  CAST({1 AS FLOAT)}
            ";

            AssertIsNotMatch(query);
        }
    }
}