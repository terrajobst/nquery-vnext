using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
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

        [TestMethod]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInTable()
        {
            var query = @"
                SELECT  *
                FROM    {Employees} e
             ";

            AssertIsMatch(query);
        }

        [TestMethod]
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