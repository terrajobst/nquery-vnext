using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
    public class CountAllSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new CountAllSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var symbol = dataContext.Aggregates.Single(a => a.Name == "COUNT");
            yield return symbol.ToSignatureItem();
        }

        [TestMethod]
        public void CountAllSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {COUNT({*})}
            ";

            AssertIsMatch(query);
        }
    }
}