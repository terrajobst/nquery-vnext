using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

namespace NQuery.Authoring.UnitTests.SignatureHelp.Providers
{
    [TestClass]
    public class FunctionSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new FunctionSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var symbols = dataContext.Functions.Where(f => f.Name == "SUBSTRING").OrderBy(f => f.Parameters.Count);
            return symbols.ToSignatureItems();
        }

        [TestMethod]
        public void FunctionSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {SUBSTRING({'a'},{ 1},{ 2})}
            ";

            AssertIsMatch(query);
        }         
    }
}