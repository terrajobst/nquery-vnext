using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
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

        [TestMethod]
        public void CastSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {CAST({100 }AS{ DOUBLE})}
            ";

            AssertIsMatch(query);
        }
    }
}