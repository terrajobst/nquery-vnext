using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class SignatureHelpModel
    {
        private readonly TextSpan _applicableSpan;
        private readonly ImmutableArray<SignatureItem> _signatures;
        private readonly SignatureItem _signature;
        private readonly int _selectedParameter;

        public SignatureHelpModel(TextSpan applicableSpan, IEnumerable<SignatureItem> signatures, SignatureItem signature, int selectedParameter)
        {
            _signatures = signatures.ToImmutableArray();
            _applicableSpan = applicableSpan;
            _signature = signature;
            _selectedParameter = selectedParameter;
        }

        public TextSpan ApplicableSpan
        {
            get { return _applicableSpan; }
        }

        public ImmutableArray<SignatureItem> Signatures
        {
            get { return _signatures; }
        }

        public SignatureItem Signature
        {
            get { return _signature; }
        }

        public int SelectedParameter
        {
            get { return _selectedParameter; }
        }

        public SignatureHelpModel WithSignature(SignatureItem signatureItem)
        {
            return new SignatureHelpModel(_applicableSpan, _signatures, signatureItem, _selectedParameter);
        }
    }
}