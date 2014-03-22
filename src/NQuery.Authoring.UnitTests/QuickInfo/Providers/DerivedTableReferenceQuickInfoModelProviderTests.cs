using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.QuickInfo.Providers
{
    [TestClass]
    public class DerivedTableReferenceQuickInfoModelProviderTests : QuickInfoModelProviderTests
    {
        protected override IQuickInfoModelProvider CreateProvider()
        {
            return new DerivedTableReferenceQuickInfoModelProvider();
        }

        protected override QuickInfoModel CreateExpectedModel(SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntax = syntaxTree.Root.DescendantNodes().OfType<DerivedTableReferenceSyntax>().Single();
            var span = syntax.Name.Span;
            var symbol = semanticModel.GetDeclaredSymbol(syntax);
            var markup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, NQueryGlyph.TableInstance, markup);
        }

        [TestMethod]
        public void DerivedTableReferenceQuickInfoModelProvider_MatchesInName()
        {
            var query = @"
                SELECT  *
                FROM    (
                            SELECT  *
                            FROM    Employees
                        ) {emps}
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void DerivedTableReferenceQuickInfoModelProvider_DoesNotMatchInParentheses()
        {
            var query = @"
                SELECT  *
                FROM    {(
                            SELECT  *
                            FROM    Employees
                        )} emps
            ";

            AssertIsNotMatch(query);
        }
    }
}