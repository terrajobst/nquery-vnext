using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching;
using NQuery.Text;

namespace NQuery.Authoring.Tests.BraceMatching
{
    public abstract class BraceMatcherTests
    {
        protected abstract IBraceMatcher CreateMatcher();

        protected void AssertIsMatch(string queryWithMarkers)
        {
            TextSpan expectedLeft;
            TextSpan expectedRight;
            var query = ParseExpectedLeftAndRight(queryWithMarkers, out expectedLeft, out expectedRight);

            var startOfLeft = Match(query, expectedLeft.Start);
            AssertIsMatch(startOfLeft, expectedLeft, expectedRight);

            var endOfLeft = Match(query, expectedLeft.End);
            AssertIsNoMatch(endOfLeft);

            var startOfRight = Match(query, expectedRight.Start);
            AssertIsNoMatch(startOfRight);

            var endOfRight = Match(query, expectedRight.End);
            AssertIsMatch(endOfRight, expectedLeft, expectedRight);
        }

        protected void AssertIsNoMatch(string queryWithMarkers)
        {
            TextSpan expectedLeft;
            TextSpan expectedRight;
            var query = ParseExpectedLeftAndRight(queryWithMarkers, out expectedLeft, out expectedRight);

            var startOfLeft = Match(query, expectedLeft.Start);
            AssertIsNoMatch(startOfLeft);

            var endOfLeft = Match(query, expectedLeft.End);
            AssertIsNoMatch(endOfLeft);

            var startOfRight = Match(query, expectedRight.Start);
            AssertIsNoMatch(startOfRight);

            var endOfRight = Match(query, expectedRight.End);
            AssertIsNoMatch(endOfRight);
        }

        private static void AssertIsMatch(BraceMatchingResult result, TextSpan expectedLeft, TextSpan expectedRight)
        {
            Assert.True(result.IsValid);
            Assert.True(result.Left.End < result.Right.Start);
            Assert.Equal(expectedLeft, result.Left);
            Assert.Equal(expectedRight, result.Right);
        }

        private static void AssertIsNoMatch(BraceMatchingResult result)
        {
            Assert.False(result.IsValid);
            Assert.Equal(default(TextSpan), result.Left);
            Assert.Equal(default(TextSpan), result.Right);
        }

        private static string ParseExpectedLeftAndRight(string queryWithMarkers, out TextSpan expectedLeft, out TextSpan expectedRight)
        {
            ImmutableArray<TextSpan> spans;
            var query = queryWithMarkers.ParseSpans(out spans);

            Assert.True(spans.Length == 2, "The query is malformed -- you need to mark two spans.");

            expectedLeft = spans[0];
            expectedRight = spans[1];
            return query;
        }

        private BraceMatchingResult Match(string query, int position)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var syntaxTree = compilation.SyntaxTree;

            var matcher = CreateMatcher();
            return syntaxTree.MatchBraces(position, new[] { matcher });
        }
    }
}