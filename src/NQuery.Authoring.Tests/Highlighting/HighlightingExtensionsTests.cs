using System;

using Xunit;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.UnitTests.Highlighting
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