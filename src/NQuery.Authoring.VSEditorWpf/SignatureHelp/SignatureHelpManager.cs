using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.SignatureHelp;
using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    internal sealed class SignatureHelpManager : ISignatureHelpManager
    {
        private readonly ITextView _textView;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private readonly ISignatureHelpModelProviderService _signatureHelpModelProviderService;
        private readonly object _selectedItemIndexKey = new();

        private ISignatureHelpSession _session;
        private SignatureHelpModel _model;

        public SignatureHelpManager(ITextView textView, ISignatureHelpBroker signatureHelpBroker, ISignatureHelpModelProviderService signatureHelpModelProviderService)
        {
            _textView = textView;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
            _signatureHelpBroker = signatureHelpBroker;
            _signatureHelpModelProviderService = signatureHelpModelProviderService;
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
            if (_session is null && text.Any(IsTriggerChar))
                TriggerSignatureHelp();
        }

        public void HandlePreviewTextInput(string text)
        {
        }

        public void TriggerSignatureHelp()
        {
            if (_session is not null)
            {
                _session.Dismiss();
            }
            else
            {
                UpdateModel();
            }
        }

        private int? GetSelectedItemIndex()
        {
            if (_session is null)
                return null;

            if (!_session.Properties.TryGetProperty(_selectedItemIndexKey, out int selectedIndex))
                return null;

            return selectedIndex;
        }

        private async void UpdateModel()
        {
            var selectedIndex = GetSelectedItemIndex();
            var documentView = _textView.GetDocumentView();
            var document = documentView.Document;
            var triggerPosition = documentView.Position;
            var semanticModel = await document.GetSemanticModelAsync();

            var model = semanticModel.GetSignatureHelpModel(triggerPosition, _signatureHelpModelProviderService.Providers);

            // If we previously recorded a selected item and the index is still valid,
            // let's restore it.

            if (model is not null && selectedIndex is not null && selectedIndex < model.Signatures.Length)
            {
                var selectedItem = model.Signatures[selectedIndex.Value];
                if (selectedItem.Parameters.Length > model.SelectedParameter)
                    model = model.WithSignature(selectedItem);
            }

            // Let observers know that we've a new model.

            Model = model;
        }

        private void UpdateSession()
        {
            if (_session is null)
                return;

            UpdateModel();
            _session.Recalculate();
            _session.Match();
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            handler?.Invoke(this, e);
        }

        public SignatureHelpModel Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                OnModelChanged(EventArgs.Empty);

                var hasData = _model is not null && _model.Signatures.Length > 0;
                var showSession = _session is null && hasData;
                var hideSession = _session is not null && !hasData;

                if (hideSession)
                {
                    _session.Dismiss();
                }
                else if (showSession)
                {
                    var snapshot = _textView.TextBuffer.CurrentSnapshot;
                    var triggerPosition = _model.ApplicableSpan.Start;
                    var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                    _session = _signatureHelpBroker.CreateSignatureHelpSession(_textView, triggerPoint, true);
                    _session.Properties.AddProperty(typeof(ISignatureHelpManager), this);
                    _session.Dismissed += SessionOnDismissed;
                    _session.SelectedSignatureChanged += SessionOnSelectedSignatureChanged;
                    _session.Start();
                    _session.Match();
                }
            }
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}