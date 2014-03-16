using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.UnitTests.BraceMatching
{
    public abstract class BraceMatcherTests
    {
        private readonly string _expectedStartText;
        private readonly string _expectedEndText;

        protected BraceMatcherTests(string expectedDelimter)
            : this(expectedDelimter, expectedDelimter)
        {
        }

        protected BraceMatcherTests(string expectedStartText, string expectedEndText)
        {
            _expectedStartText = expectedStartText;
            _expectedEndText = expectedEndText;
        }

        private BraceMatchingResult Match(string query, out int position)
        {
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var syntaxTree = compilation.SyntaxTree;

            var matcher = CreateMatcher();
            return syntaxTree.FindBrace(position, new[] { matcher });
        }

        protected abstract IBraceMatcher CreateMatcher();

        public void AssertIsMatchAtStart(string query)
        {
            AssertIsMatch(query, true);
        }

        public void AssertIsMatchAtEnd(string query)
        {
            AssertIsMatch(query, false);
        }

        private void AssertIsMatch(string query, bool atStart)
        {
            int position;
            var result = Match(query, out position);
            var queryWithoutPipe = query.Replace("|", string.Empty);

            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Left.End < result.Right.Start);
            Assert.AreEqual(position, atStart ? result.Left.Start : result.Right.End);
            Assert.AreEqual(_expectedStartText, queryWithoutPipe.Substring(result.Left));
            Assert.AreEqual(_expectedEndText, queryWithoutPipe.Substring(result.Right));
        }

        protected void AssertIsNoMatch(string query)
        {
            int position;
            var result = Match(query, out position);

            Assert.IsFalse(result.IsValid);
        }        
    }
}