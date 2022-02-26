using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.Tests.SignatureHelp
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