using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class FunctionSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new FunctionSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        var symbols = semanticModel.Functions
                                   .Where(f => f.Name == "SUBSTRING")
                                   .OrderBy(f => f.Parameters.Length);
        return symbols.ToSignatureItems();
    }

    [Fact]
    public void FunctionSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT {SUBSTRING({'a'},{ 1},{ 2})}
            """;

        AssertIsMatch(query);
    }
}
