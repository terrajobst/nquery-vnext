using System;

using Xunit;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp
{
    public class SignatureHelpExtensionsTests : ExtensionTests
    {
        [Fact]
        public void SignatureHelpExtensions_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(SignatureHelpExtensions.GetStandardSignatureHelpModelProviders);
        }
    }
}