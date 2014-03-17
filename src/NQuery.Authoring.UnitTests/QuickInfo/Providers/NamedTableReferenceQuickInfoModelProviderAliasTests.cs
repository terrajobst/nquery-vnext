using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class NamedTableReferenceQuickInfoModelProviderAliasTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NamedTableReferenceQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single();
            var span = syntax.Alias.Span;
            var symbol = semanticModel.GetDeclaredSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.TableInstance,  markup);
        }

        [TestMethod]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInAlias()
        {
            var query = @"
                SELECT  *
                FROM    Employees {e}
             ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInAliasUnresolved()
        {
            var query = @"
                SELECT  *
                FROM    Xxxxxxxxx {e}
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void NamedTableReferenceQuickInfoModelProvider_DoesNotMatchInAs()
        {
            var query = @"
                SELECT  *
                FROM    Employees {AS} e
            ";

            AssertIsNotMatch(query);
        }
    }
}