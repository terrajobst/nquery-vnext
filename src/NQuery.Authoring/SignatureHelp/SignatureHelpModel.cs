using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.SignatureHelp;

public sealed class SignatureHelpModel
{
    public SignatureHelpModel(TextSpan applicableSpan, IEnumerable<SignatureItem> signatures, SignatureItem signature, int selectedParameter)
    {
        ThrowIfNull(signatures);
        ThrowIfNull(signature);

        Signatures = [.. signatures];
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
        ThrowIfNull(signatureItem);

        return new SignatureHelpModel(ApplicableSpan, Signatures, signatureItem, SelectedParameter);
    }
}
