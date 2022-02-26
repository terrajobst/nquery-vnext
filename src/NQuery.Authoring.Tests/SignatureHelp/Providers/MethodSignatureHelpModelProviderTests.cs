using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers
{
    public class MethodSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
    {
        protected override ISignatureHelpModelProvider CreateProvider()
        {
            return new MethodSignatureHelpModelProvider();
        }

        protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
        {
            var methods = semanticModel.LookupMethods(typeof(string)).Where(m => m.Name == "Substring").OrderBy(m => m.Parameters.Length);
            return methods.ToSignatureItems();
        }

        [Fact]
        public void MethodSignatureHelpModelProvider_Matches()
        {
            var query = @"
                SELECT {'a'.Substring({ 1},{ 2})}
            ";

            AssertIsMatch(query);
        }
    }
}