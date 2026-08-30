using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class NullIfSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new NullIfSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        yield return SignatureHelpExtensions.GetNullIfSignatureItem();
    }

    [Fact]
    public void NullIfSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT  {NULLIF({e.EmployeeId},{ 1})}
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }
}
