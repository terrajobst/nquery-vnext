using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

using Xunit;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers
{
    public class NullIfSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new NullIfSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            yield return SignatureHelpExtensions.GetNullIfSignatureItem();
        }

        [Fact]
        public void NullIfSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT  {NULLIF({e.EmployeeId},{ 1})}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}