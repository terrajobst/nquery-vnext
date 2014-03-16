using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class IdentifierBraceMatcherTests : BraceMatcherTests
    {
        public IdentifierBraceMatcherTests()
            : base(string.Empty)
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoestNotMatchInMiddleOfRegularIdentifier()
        {
            var query = @"
                SELECT  First|Name
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoestNotMatchAtStartOfRegularIdentifier()
        {
            var query = @"
                SELECT  |FirstName
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoestNotMatchAtEndOfRegularIdentifier()
        {
            var query = @"
                SELECT  FirstName|
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}