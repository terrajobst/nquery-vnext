using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.SignatureHelp.Providers;

public class CoalesceSignatureHelpProviderTests : SignatureHelpProviderTests
{
    protected override ISignatureHelpProvider CreateProvider()
    {
        return new CoalesceSignatureHelpProvider();
    }

    protected override IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel)
    {
        yield return SignatureHelpExtensions.GetCoalesceSignatureItem();
    }

    [Fact]
    public void CoalesceSignatureHelpProvider_Matches()
    {
        var query = """
            SELECT  {COALESCE({e.ReportsTo},{ e.EmployeeId})}
            FROM    Employees e
            """;

        AssertIsMatch(query);
    }
}
