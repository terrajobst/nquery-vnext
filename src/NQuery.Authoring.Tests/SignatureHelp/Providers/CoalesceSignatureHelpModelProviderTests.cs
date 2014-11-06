using System.Collections.Generic;

using Xunit;

using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    public class CoalesceSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new CoalesceSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            yield return SignatureHelpExtensions.GetCoalesceSignatureItem();
        }

        [Fact]
        public void CoalesceSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT  {COALESCE({e.ReportsTo},{ e.EmployeeId})}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}