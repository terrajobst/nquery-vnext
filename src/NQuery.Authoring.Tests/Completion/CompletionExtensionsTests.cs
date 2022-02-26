using NQuery.Authoring.Completion;

namespace NQuery.Authoring.Tests.Completion
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