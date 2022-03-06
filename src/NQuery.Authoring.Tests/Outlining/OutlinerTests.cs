using System.Collections.Immutable;

using NQuery.Authoring.Outlining;
using NQuery.Text;

namespace NQuery.Authoring.Tests.Outlining
{
    public abstract class OutlinerTests
    {
        protected abstract IOutliner CreateOutliner();

        protected void AssertIsNoMatch(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var root = compilation.SyntaxTree.Root;

            var outliner = CreateOutliner();
            var outliners = ImmutableArray.Create(outliner);

            var actualRegions = root.FindRegions(root.FullSpan, outliners);

            Assert.Empty(actualRegions);
        }

        protected void AssertIsMatch(string queryWithMarkers, string expectedText)
        {
            var query = queryWithMarkers.ParseSingleSpan(out var expectedSpan);

            var compilation = CompilationFactory.CreateQuery(query);
            var root = compilation.SyntaxTree.Root;

            var outliner = CreateOutliner();
            var outliners = ImmutableArray.Create(outliner);

            AssertIsMatch(root, outliners, expectedSpan, expectedText);
        }

        private static void AssertIsMatch(SyntaxNode root, ImmutableArray<IOutliner> outliners, TextSpan expectedSpan, string expectedText)
        {
            var documentSpan = root.SyntaxTree.Root.FullSpan;

            AssertMatches(root, documentSpan, outliners, expectedSpan, expectedText);
            AssertMatches(root, expectedSpan, outliners, expectedSpan, expectedText);
        }

        private static void AssertMatches(SyntaxNode root, TextSpan span, ImmutableArray<IOutliner> outliners, TextSpan expectedSpan, string expectedText)
        {
            var actualRegions = root.FindRegions(span, outliners).ToImmutableArray();

            var actualRegion = Assert.Single(actualRegions);
            Assert.Equal(expectedSpan, actualRegion.Span);
            Assert.Equal(expectedText, actualRegion.Text);
        }
    }
}