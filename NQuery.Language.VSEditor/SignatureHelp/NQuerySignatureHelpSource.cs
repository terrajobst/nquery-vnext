using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    internal class NQuerySignatureHelpSource : ISignatureHelpSource
    {
        private readonly ITextBuffer _textBuffer;
        private bool _isDisposed;

        public NQuerySignatureHelpSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            ISignatureHelpManager signatureHelpManager;
            if (!session.Properties.TryGetProperty(typeof(ISignatureHelpManager), out signatureHelpManager))
                return;

            var model = signatureHelpManager.Model;
            if (model == null)
                return;

            var snapshot = _textBuffer.CurrentSnapshot;
            var span = model.ApplicableSpan;
            var trackingSpan = snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeExclusive);

            var signaturesMap = ToSignatures(trackingSpan, model.Signatures, model.SelectedParameter);
            var signatureMapKey = typeof (Dictionary<SignatureItem, ISignature>);
            session.Properties.RemoveProperty(signatureMapKey);
            session.Properties.AddProperty(signatureMapKey, signaturesMap);

            foreach (var signature in model.Signatures)
                signatures.Add(signaturesMap[signature]);
        }

        private Dictionary<SignatureItem, ISignature> ToSignatures(ITrackingSpan applicableSpan, IEnumerable<SignatureItem> signatures, int selectedParameter)
        {
            return signatures.ToDictionary(s => s, s => (ISignature)new NQuerySignature(applicableSpan, s, selectedParameter));
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            ISignatureHelpManager signatureHelpManager;
            if (!session.Properties.TryGetProperty(typeof(ISignatureHelpManager), out signatureHelpManager))
                return null;

            Dictionary<SignatureItem, ISignature> signaturesMap;
            if (!session.Properties.TryGetProperty(typeof(Dictionary<SignatureItem, ISignature>), out signaturesMap))
                return null;

            var model = signatureHelpManager.Model;
            if (model == null)
                return null;

            if (model.Signature == null)
                return null;

            return signaturesMap[model.Signature];
        }
    }
}