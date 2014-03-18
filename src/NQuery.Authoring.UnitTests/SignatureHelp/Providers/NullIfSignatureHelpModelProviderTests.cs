using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
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

        [TestMethod]
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