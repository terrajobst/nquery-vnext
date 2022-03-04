using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Tests.QuickInfo.Providers
{
    public class NamedTableReferenceQuickInfoModelProviderAliasTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new NamedTableReferenceQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single();
            var span = syntax.Alias.Span;
            var symbol = semanticModel.GetDeclaredSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, Glyph.TableInstance, markup);
        }

        [Fact]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInAlias()
        {
            var query = @"
                SELECT  *
                FROM    Employees {e}
             ";

            AssertIsMatch(query);
        }

        [Fact]
        public void NamedTableReferenceQuickInfoModelProvider_MatchesInAliasUnresolved()
        {
            var query = @"
                SELECT  *
                FROM    Xxxxxxxxx {e}
            ";

            AssertIsMatch(query);
        }

        [Fact]
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