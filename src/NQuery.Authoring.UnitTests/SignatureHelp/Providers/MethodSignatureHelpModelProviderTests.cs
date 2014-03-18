using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
    public class MethodSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new MethodSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            var methods = semanticModel.LookupMethods(typeof(string)).Where(m => m.Name == "Substring").OrderBy(m => m.Parameters.Count);
            return methods.ToSignatureItems();
        }

        [TestMethod]
        public void MethodSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {'a'.Substring({ 1},{ 2})}
            ";

            AssertIsMatch(query);
        }         
    }
}