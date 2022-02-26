using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.Tests.Highlighting
{
    public class HighlightingExtensionsTests : ExtensionTests
    {
        [Fact]
        public void HighlightingExtensions_ReturnsAllHighlighters()
        {
            AssertAllProvidersAreExposed(HighlightingExtensions.GetStandardHighlighters);
        }
    }
}