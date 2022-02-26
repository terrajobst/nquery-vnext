using NQuery.Authoring.BraceMatching;

using Xunit;

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