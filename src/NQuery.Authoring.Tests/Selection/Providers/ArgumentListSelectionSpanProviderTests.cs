using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;
using Xunit;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class ArgumentListSelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new ArgumentListSelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_ArgumentList()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.FirstName.Substring({{{1},} 2}) = 'te'
            ";

            AssertIsMatch(query);
        }
    }
}