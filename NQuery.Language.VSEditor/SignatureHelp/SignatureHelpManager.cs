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
        private readonly object _selectedItemIndexKey = new object();

        private ISignatureHelpSession _session;
        private SignatureHelpModel _model;

        public SignatureHelpManager(ITextView textView, INQuerySemanticModelManager semanticModelManager, ISignatureHelpBroker signatureHelpBroker, IEnumerable<ISignatureModelProvider> signatureModelProviders)
        {
            _textView = textView;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
            _semanticModelManager = semanticModelManager;
            _signatureHelpBroker = signatureHelpBroker;
            _signatureModelProviders = signatureModelProviders;
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _session.SelectedSignatureChanged -= SessionOnDismissed;
            _session = null;
        }

        private void SessionOnSelectedSignatureChanged(object sender, SelectedSignatureChangedEventArgs e)
        {
            var selectedItemIndex = _session.Signatures.IndexOf(e.NewSelectedSignature);

            _session.Properties.RemoveProperty(_selectedItemIndexKey);
            _session.Properties.AddProperty(_selectedItemIndexKey, selectedItemIndex);
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateSession();
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
        {
            UpdateSession();
        }

        private static bool IsTriggerChar(char c)
        {
            return c == '(' ||
                   c == ',';
        }

        public void HandleTextInput(string text)
        {
            if (_session == null && text.Any(IsTriggerChar))
                TriggerSignatureHelp();
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
                    _session.SelectedSignatureChanged += SessionOnSelectedSignatureChanged;
                    _session.Start();
                }
            }
        }

        private int? GetSelectedItemIndex()
        {
            if (_session == null)
                return null;

            int selectedIndex;
            if (!_session.Properties.TryGetProperty(_selectedItemIndexKey, out selectedIndex))
                return null;

            return selectedIndex;
        }

        private void UpdateModel()
        {
            var selectedIndex = GetSelectedItemIndex();
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

            // If we previously recorded a selected item and the index is still valid,
            // let's restore it.

            if (model != null && selectedIndex != null && selectedIndex < model.Signatures.Count)
            {
                var selectedItem = model.Signatures[selectedIndex.Value];
                model = model.WithSignature(selectedItem);
            }

            // Let observers know that we've a new model.

            Model = model;
        }

        private void UpdateSession()
        {
            if (_session == null)
                return;

            UpdateModel();
            _session.Recalculate();
            _session.Match();
        }

        private void OnModelChanged(EventArgs e)
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