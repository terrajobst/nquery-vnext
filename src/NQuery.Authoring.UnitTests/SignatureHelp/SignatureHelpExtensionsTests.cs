using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp
{
    [TestClass]
    public class SignatureHelpExtensionsTests : ExtensionTests
    {
        [TestMethod]
        public void SignatureHelpExtensions_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(SignatureHelpExtensions.GetStandardSignatureHelpModelProviders);
        }
    }
}