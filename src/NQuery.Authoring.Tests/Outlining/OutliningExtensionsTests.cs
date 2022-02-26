using NQuery.Authoring.Outlining;

using Xunit;

namespace NQuery.Authoring.Tests.Outlining
{
    public class OutliningExtensionsTests : ExtensionTests
    {
        [Fact]
        public void OutliningExtensions_ReturnsAllOutliners()
        {
            AssertAllProvidersAreExposed(OutliningExtensions.GetStandardOutliners);
        }
    }
}