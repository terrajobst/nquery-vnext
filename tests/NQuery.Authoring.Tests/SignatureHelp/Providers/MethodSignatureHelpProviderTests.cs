using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class MethodSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new MethodSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        var methods = semanticModel.LookupMethods(typeof(string)).Where(m => m.Name == "Substring").OrderBy(m => m.Parameters.Length);
        return methods.ToSignatureItems();
    }

    [Fact]
    public void MethodSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT {'a'.Substring({ 1},{ 2})}
            """;

        AssertIsMatch(query);
    }
}
