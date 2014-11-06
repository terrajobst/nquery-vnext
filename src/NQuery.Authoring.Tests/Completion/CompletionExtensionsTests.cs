using System;

using Xunit;

using NQuery.Authoring.Completion;

namespace NQuery.Authoring.UnitTests.Completion
{
    public class CompletionExtensionsTests : ExtensionTests
    {
        [Fact]
        public void CompletionExtensionsTests_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(CompletionExtensions.GetStandardCompletionProviders);
        }
    }
}