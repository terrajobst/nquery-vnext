using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.UnitTests.BraceMatching
{
    [TestClass]
    public class BraceMatchingExtensionsTests
    {
        [TestMethod]
        public void BraceMatchingExtensions_ReturnsAllBraceMatchers()
        {
            var allTypes = typeof(IBraceMatcher).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IBraceMatcher).IsAssignableFrom(t)).ToArray();
            var matchers = BraceMatchingExtensions.GetStandardBraceMatchers().Select(t => t.GetType()).ToArray();
            var exposedTypes = new HashSet<Type>(matchers);

            foreach (var type in allTypes)
                Assert.IsTrue(exposedTypes.Contains(type), "Brace matcher {0} isn't exposed from GetStandardBraceMatchers()", type);

            Assert.AreEqual(matchers.Length, allTypes.Length);
        }
    }
}