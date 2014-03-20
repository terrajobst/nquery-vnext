using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Completion;

namespace NQuery.Authoring.UnitTests.Completion
{
    [TestClass]
    public class CompletionExtensionsTests : ExtensionTests
    {
        [TestMethod]
        public void CompletionExtensionsTests_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(CompletionExtensions.GetStandardCompletionProviders);
        }
    }
}