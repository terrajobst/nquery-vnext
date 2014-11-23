using System;
using System.Linq;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class NamedTableReferenceQuickInfoModelProviderTableTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NamedTableReferenceQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single();
            var span = syntax.TableName.Span;
            var symbol = semanticModel.GetDeclaredSymbol(syntax).Table;
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.Table, markup);
        }

        [Fact]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInTable()
        {
            var query = @"
                SELECT  *
                FROM    {Employees} e
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void NamedTableReferenceQuickInfoModelProvider_DoesNotMatchUnresolved()
        {
            var query = @"
                SELECT  *
                FROM    {Xxxxxxxxx} e
            ";

            AssertIsNotMatch(query);
        }
    }
}