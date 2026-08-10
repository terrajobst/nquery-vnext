using System.Collections.Immutable;

using NQuery.Authoring.SignatureHelp;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.SignatureHelp;

public abstract class SignatureHelpModelProviderTests
{
    protected abstract ISignatureHelpModelProvider CreateProvider();

    protected abstract IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel);

    protected void AssertIsMatch(string queryWithMarkers)
    {
        var query = queryWithMarkers.ParseSpans(out var spans);

        var applicableSpan = spans[0];
        var parameterSpans = spans.Skip(1).ToImmutableArray();

        var services = DocumentFactory.ServicesWithOnly(CreateProvider());
        var document = DocumentFactory.CreateQuery(query, services);

        for (var i = 0; i < parameterSpans.Length; i++)
        {
            var parameterSpan = parameterSpans[i];
            var start = parameterSpan.Start;
            var middle = parameterSpan.Start + parameterSpan.Length / 2;
            var end = parameterSpan.Start;

            AssertIsMatch(document, start, applicableSpan, i);
            AssertIsMatch(document, middle, applicableSpan, i);
            AssertIsMatch(document, end, applicableSpan, i);
        }
    }

    private void AssertIsMatch(Document document, int position, TextSpan expectedApplicableSpan, int expectedSelectedParameter)
    {
        var view = DocumentView.Create(document, position);
        var actualModel = document.Services.GetService<SignatureHelpService>().GetModel(view)!;
        var expectedSignatures = GetExpectedSignatures(document.GetSemanticModel()).ToImmutableArray();

        Assert.Equal(expectedApplicableSpan, actualModel.ApplicableSpan);
        Assert.Equal(expectedSelectedParameter, actualModel.SelectedParameter);
        Assert.Equal(expectedSignatures.AsEnumerable(), actualModel.Signatures.AsEnumerable());
    }
}
