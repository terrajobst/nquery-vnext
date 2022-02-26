using System.Collections.Immutable;

using NQuery.Authoring.SignatureHelp;
using NQuery.Text;

using Xunit;

namespace NQuery.Authoring.Tests.SignatureHelp
{
    public abstract class SignatureHelpModelProviderTests
    {
        protected abstract ISignatureHelpModelProvider CreateProvider();

        protected abstract IEnumerable<SignatureItem> GetExpectedSignatures(SemanticModel semanticModel);

        protected void AssertIsMatch(string queryWithMarkers)
        {
            ImmutableArray<TextSpan> spans;
            var query = queryWithMarkers.ParseSpans(out spans);

            var applicableSpan = spans[0];
            var parameterSpans = spans.Skip(1).ToImmutableArray();

            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] { provider };

            for (var i = 0; i < parameterSpans.Length; i++)
            {
                var parameterSpan = parameterSpans[i];
                var start = parameterSpan.Start;
                var middle = parameterSpan.Start + parameterSpan.Length/2;
                var end = parameterSpan.Start;

                AssertIsMatch(semanticModel, start, providers, applicableSpan, i);
                AssertIsMatch(semanticModel, middle, providers, applicableSpan, i);
                AssertIsMatch(semanticModel, end, providers, applicableSpan, i);
            }
        }

        private void AssertIsMatch(SemanticModel semanticModel, int position, IEnumerable<ISignatureHelpModelProvider> providers, TextSpan expectedApplicableSpan, int expectedSelectedParameter)
        {
            var actualModel = semanticModel.GetSignatureHelpModel(position, providers);
            var expectedSignatures = GetExpectedSignatures(semanticModel).ToImmutableArray();

            Assert.Equal(expectedApplicableSpan, actualModel.ApplicableSpan);
            Assert.Equal(expectedSelectedParameter, actualModel.SelectedParameter);
            Assert.Equal(expectedSignatures.AsEnumerable(), actualModel.Signatures.AsEnumerable());
        }
    }
}