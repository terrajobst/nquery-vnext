using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class CountAllSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new CountAllSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        var symbol = semanticModel.Aggregates.Single(a => a.Name == "COUNT");
        yield return symbol.ToSignatureItem();
    }

    [Fact]
    public void CountAllSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT {COUNT({*})}
            """;

        AssertIsMatch(query);
    }
}
