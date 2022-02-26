using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

using Xunit;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers
{
    public class CastSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new CastSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            yield return SignatureHelpExtensions.GetCastSignatureItem();
        }

        [Fact]
        public void CastSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {CAST({100 }AS{ DOUBLE})}
            ";

            AssertIsMatch(query);
        }
    }
}