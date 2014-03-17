using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class WildcardSelectColumnQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new WildcardSelectColumnQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<WildcardSelectColumnSyntax>().Single();
            var span = syntax.TableName.Span;
            var symbol = semanticModel.GetTableInstance(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.TableInstance, markup);
        }

        [TestMethod]
        public void WildcardSelectColumnQuickInfoModelProvider_MatchesInAlias()
        {
            var query = @"
                SELECT  {e}.*
                FROM    Employees e
             ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void WildcardSelectColumnQuickInfoModelProvider_DoesNotMatchesUnresolved()
        {
            var query = @"
                SELECT  {x}.*
                FROM    Employees e
            ";

            AssertIsNotMatch(query);
        }

        [TestMethod]
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