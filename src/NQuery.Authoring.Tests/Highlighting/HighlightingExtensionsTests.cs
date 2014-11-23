using System;

using NQuery.Authoring.Highlighting;

using Xunit;

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