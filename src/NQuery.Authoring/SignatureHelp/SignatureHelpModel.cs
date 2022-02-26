using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class SignatureHelpModel
    {
        public SignatureHelpModel(TextSpan applicableSpan, IEnumerable<SignatureItem> signatures, SignatureItem signature, int selectedParameter)
        {
            Signatures = signatures.ToImmutableArray();
            ApplicableSpan = applicableSpan;
            Signature = signature;
            SelectedParameter = selectedParameter;
        }

        public TextSpan ApplicableSpan { get; }

        public ImmutableArray<SignatureItem> Signatures { get; }

        public SignatureItem Signature { get; }

        public int SelectedParameter { get; }

        public SignatureHelpModel WithSignature(SignatureItem signatureItem)
        {
            return new SignatureHelpModel(ApplicableSpan, Signatures, signatureItem, SelectedParameter);
        }
    }
}