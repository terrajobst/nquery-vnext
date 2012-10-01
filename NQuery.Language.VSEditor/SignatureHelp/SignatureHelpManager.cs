using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    internal sealed class SignatureHelpManager : ISignatureHelpManager
    {
        private readonly ITextView _textView;
        private readonly INQuerySemanticModelManager _semanticModelManager;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private readonly IEnumerable<ISignatureModelProvider> _signatureModelProviders;

        private ISignatureHelpSession _session;
        private SignatureHelpModel _model;

        public SignatureHelpManager(ITextView textView, INQuerySemanticModelManager semanticModelManager, ISignatureHelpBroker signatureHelpBroker, IEnumerable<ISignatureModelProvider> signatureModelProviders)
        {
            _textView = textView;
            _semanticModelManager = semanticModelManager;
            _signatureHelpBroker = signatureHelpBroker;
            _signatureModelProviders = signatureModelProviders;
        }

        private static bool IsTriggerChar(char c)
        {
            return c == '(';
        }

        private static bool IsCommitChar(char c)
        {
            return c == ')';
        }

        public void HandleTextInput(string text)
        {
            if (_session == null && text.Any(IsTriggerChar))
                TriggerSignatureHelp();
            else if (_session != null && text.Any(IsCommitChar))
                _session.Dismiss();
        }

        public void HandlePreviewTextInput(string text)
        {
        }

        public void TriggerSignatureHelp()
        {
            if (_session != null)
            {
                _session.Dismiss();
            }
            else
            {
                UpdateModel();

                if (_model.Signatures.Count > 0)
                {
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var triggerPosition = _model.ApplicableSpan.Start;
                    var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                    _session = _signatureHelpBroker.CreateSignatureHelpSession(_textView, triggerPoint, true);
                    _session.Properties.AddProperty(typeof(ISignatureHelpManager), this);
                    _session.Dismissed += SessionOnDismissed;
                    _session.Start();
                }
            }
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        private void UpdateModel()
        {
            var textView = _textView;
            var triggerPosition = textView.Caret.Position.BufferPosition.Position;
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var oldCompilation = _semanticModelManager.Compilation;

            // TODO: The following should be done as a (cancellable) background operation.
            // 
            // It needs to be cancellable because before starting a new one, the old one
            // needs to be cancelled in order to avoid stale results.

            var source = snapshot.GetText();
            var syntaxTree = SyntaxTree.ParseQuery(source);

            var compilation = oldCompilation.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var model = _signatureModelProviders.Select(p => p.GetModel(semanticModel, triggerPosition))
                                                .Where(m => m != null)
                                                .OrderByDescending(m => m.ApplicableSpan.Start)
                                                .FirstOrDefault();

            // Let observers know that we've a new model.

            Model = model;
        }

        internal void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, e);
        }

        public SignatureHelpModel Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                OnModelChanged(EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}