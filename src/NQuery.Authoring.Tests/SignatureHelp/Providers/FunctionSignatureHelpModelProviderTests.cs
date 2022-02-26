using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

using Xunit;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers
{
    public class FunctionSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new FunctionSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var symbols = dataContext.Functions.Where(f => f.Name == "SUBSTRING").OrderBy(f => f.Parameters.Length);
            return symbols.ToSignatureItems();
        }

        [Fact]
        public void FunctionSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {SUBSTRING({'a'},{ 1},{ 2})}
            ";

            AssertIsMatch(query);
        }
    }
}