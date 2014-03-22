using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.UnitTests.Highlighting
{
    [TestClass]
    public class HighlightingExtensionsTests : ExtensionTests
    {
        [TestMethod]
        public void HighlightingExtensions_ReturnsAllHighlighters()
        {
            AssertAllProvidersAreExposed(HighlightingExtensions.GetStandardHighlighters);
        }
    }
}