using System;

using Xunit;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.UnitTests.BraceMatching
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