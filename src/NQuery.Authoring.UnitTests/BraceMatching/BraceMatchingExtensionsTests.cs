using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.UnitTests.BraceMatching
{
    [TestClass]
    public class BraceMatchingExtensionsTests : ExtensionTests
    {
        [TestMethod]
        public void BraceMatchingExtensions_ReturnsAllBraceMatchers()
        {
            AssertAllProvidersAreExposed(BraceMatchingExtensions.GetStandardBraceMatchers);
        }
    }
}