using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.Services.SignatureHelp
{
    public sealed class SignatureHelpModel
    {
        private readonly TextSpan _applicableSpan;
        private readonly ReadOnlyCollection<SignatureItem> _signatures;
        private readonly SignatureItem _signature;
        private readonly int _selectedParameter;

        public SignatureHelpModel(TextSpan applicableSpan, IList<SignatureItem> signatures, SignatureItem signature, int selectedParameter)
        {
            _signatures = new ReadOnlyCollection<SignatureItem>(signatures);
            _applicableSpan = applicableSpan;
            _signature = signature;
            _selectedParameter = selectedParameter;
        }

        public TextSpan ApplicableSpan
        {
            get { return _applicableSpan; }
        }

        public ReadOnlyCollection<SignatureItem> Signatures
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