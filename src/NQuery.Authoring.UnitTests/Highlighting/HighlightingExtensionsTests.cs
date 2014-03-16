using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.UnitTests.Highlighting
{
    [TestClass]
    public class HighlightingExtensionsTests
    {
        [TestMethod]
        public void HighlightingExtensions_ReturnsAllHighlighters()
        {
            var allTypes = typeof(IHighlighter).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IHighlighter).IsAssignableFrom(t)).ToArray();
            var matchers = HighlightingExtensions.GetStandardHighlighters().Select(t => t.GetType()).ToArray();
            var exposedTypes = new HashSet<Type>(matchers);

            foreach (var type in allTypes)
                Assert.IsTrue(exposedTypes.Contains(type), "Highlighter {0} isn't exposed from GetStandardHighlighters()", type);

            Assert.AreEqual(matchers.Length, allTypes.Length);
        }
    }
}