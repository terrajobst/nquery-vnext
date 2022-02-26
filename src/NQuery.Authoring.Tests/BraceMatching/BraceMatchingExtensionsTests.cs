using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.Tests.BraceMatching
{
    public class BraceMatchingExtensionsTests : ExtensionTests
    {
        [Fact]
        public void BraceMatchingExtensions_ReturnsAllBraceMatchers()
        {
            AssertAllProvidersAreExposed(BraceMatchingExtensions.GetStandardBraceMatchers);
        }
    }
}