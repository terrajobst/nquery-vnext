using System;
using System.Collections.Immutable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Text;

namespace NQuery.Authoring.UnitTests.BraceMatching
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
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Left.End < result.Right.Start);
            Assert.AreEqual(expectedLeft, result.Left);
            Assert.AreEqual(expectedRight, result.Right);
        }

        private static void AssertIsNoMatch(BraceMatchingResult result)
        {
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(default(TextSpan), result.Left);
            Assert.AreEqual(default(TextSpan), result.Right);
        }

        private static string ParseExpectedLeftAndRight(string queryWithMarkers, out TextSpan expectedLeft, out TextSpan expectedRight)
        {
            ImmutableArray<TextSpan> spans;
            var query = queryWithMarkers.ParseSpans(out spans);

            if (spans.Length != 2)
                Assert.Inconclusive("The query is malformed -- you need to mark two spans.");

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