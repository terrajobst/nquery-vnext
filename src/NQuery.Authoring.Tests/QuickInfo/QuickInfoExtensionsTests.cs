using System;

using NQuery.Authoring.QuickInfo;

using Xunit;

namespace NQuery.Authoring.Tests.QuickInfo
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