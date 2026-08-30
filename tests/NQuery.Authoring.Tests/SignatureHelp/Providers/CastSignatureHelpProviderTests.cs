using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class CastSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new CastSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        yield return SignatureHelpExtensions.GetCastSignatureItem();
    }

    [Fact]
    public void CastSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT {CAST({100 }AS{ DOUBLE})}
            """;

        AssertIsMatch(query);
    }
}
