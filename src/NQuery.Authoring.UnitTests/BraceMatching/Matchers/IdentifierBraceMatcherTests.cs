using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class IdentifierBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoestNotMatchRegularIdentifier()
        {
            var query = @"
                SELECT  {}FirstName{}
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}