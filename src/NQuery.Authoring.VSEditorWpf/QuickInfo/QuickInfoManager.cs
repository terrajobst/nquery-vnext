using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.QuickInfo;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.VSEditorWpf.Text;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    internal sealed class QuickInfoManager : IQuickInfoManager
    {
        private readonly Workspace _workspace;
        private readonly ITextView _textView;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly IQuickInfoModelProviderService _quickInfoModelProviderService;

        private QuickInfoModel _model;
        private IQuickInfoSession _session;

        public QuickInfoManager(Workspace workspace, ITextView textView, IQuickInfoBroker quickInfoBroker, IQuickInfoModelProviderService quickInfoModelProviderService)
        {
            _workspace = workspace;
            _textView = textView;
            _quickInfoBroker = quickInfoBroker;
            _quickInfoModelProviderService = quickInfoModelProviderService;
        }

        public async void TriggerQuickInfo(int offset)
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            Model = semanticModel.GetQuickInfoModel(offset, _quickInfoModelProviderService.Providers);
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            handler?.Invoke(this, e);
        }

        public QuickInfoModel Model
        {
            get { return _model; }
            private set
            {
                if (_model != value)
                {
                    _model = value;
                    OnModelChanged(EventArgs.Empty);

                    var hasData = _model is not null;
                    var showSession = _session is null && hasData;
                    var hideSession = _session is not null && !hasData;

                    if (hideSession)
                    {
                        _session.Dismiss();
                    }
                    else if (showSession)
                    {
                        var syntaxTree = _model.SemanticModel.SyntaxTree;
                        var snapshot = syntaxTree.Text.ToTextSnapshot();
                        var triggerPosition = _model.Span.Start;
                        var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                        _session = _quickInfoBroker.CreateQuickInfoSession(_textView, triggerPoint, true);
                        _session.Properties.AddProperty(typeof(IQuickInfoManager), this);
                        _session.Dismissed += SessionOnDismissed;
                        _session.Start();
                    }
                }
            }
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}