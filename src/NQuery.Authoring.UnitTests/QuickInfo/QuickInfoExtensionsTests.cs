using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.UnitTests.QuickInfo
{
    [TestClass]
    public class QuickInfoExtensionsTests : ExtensionTests
    {
        [TestMethod]
        public void QuickInfoExtensions_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(QuickInfoExtensions.GetStandardQuickInfoModelProviders);
        }
    }
}