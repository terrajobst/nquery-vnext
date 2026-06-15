using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.Symbols;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class CountAllSignatureHelpModelProviderTests : SignatureHelpModelProviderTests
{
    protected override ISignatureHelpModelProvider CreateProvider()
    {
        return new CountAllSignatureHelpModelProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        var symbol = semanticModel.Aggregates.Single(a => a.Name == "COUNT");
        yield return symbol.ToSignatureItem();
    }

    [Fact]
    public void CountAllSignatureHelpModelProvider_Matches()
    {
        var query = @"
                SELECT {COUNT({*})}
            ";

        AssertIsMatch(query);
    }
}
