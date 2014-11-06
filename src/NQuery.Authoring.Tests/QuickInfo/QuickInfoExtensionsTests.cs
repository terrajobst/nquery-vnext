using System;

using Xunit;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.UnitTests.QuickInfo
{
    public class QuickInfoExtensionsTests : ExtensionTests
    {
        [Fact]
        public void QuickInfoExtensions_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(QuickInfoExtensions.GetStandardQuickInfoModelProviders);
        }
    }
}