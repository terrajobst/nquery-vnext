using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.UnitTests.QuickInfo
{
    [TestClass]
    public class QuickInfoExtensionsTests
    {
        [TestMethod]
        public void QuickInfoExtensions_ReturnsAllProviders()
        {
            var allTypes = typeof(IQuickInfoModelProvider).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IQuickInfoModelProvider).IsAssignableFrom(t)).ToArray();
            var matchers = QuickInfoExtensions.GetStandardQuickInfoModelProviders().Select(t => t.GetType()).ToArray();
            var exposedTypes = new HashSet<Type>(matchers);

            foreach (var type in allTypes)
                Assert.IsTrue(exposedTypes.Contains(type), "Provider {0} isn't exposed from GetStandardQuickInfoModelProviders()", type);

            Assert.AreEqual(matchers.Length, allTypes.Length);
        }
    }
}