using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
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

        [TestMethod]
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