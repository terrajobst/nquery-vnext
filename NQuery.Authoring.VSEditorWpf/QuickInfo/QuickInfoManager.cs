using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language.Services.QuickInfo;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.QuickInfo
{
    internal sealed class QuickInfoManager : IQuickInfoManager
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly IEnumerable<IQuickInfoModelProvider> _providers;

        private QuickInfoModel _model;
        private IQuickInfoSession _session;

        public QuickInfoManager(ITextView textView, INQueryDocument document, IQuickInfoBroker quickInfoBroker, IEnumerable<IQuickInfoModelProvider> providers)
        {
            _textView = textView;
            _document = document;
            _quickInfoBroker = quickInfoBroker;
            _providers = providers;
        }

        public async void TriggerQuickInfo(int offset)
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var model = _providers.Select(p => p.GetModel(semanticModel, offset)).FirstOrDefault(m => m != null);
            Model = model;
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, e);
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

                    var hasData = _model != null;
                    var showSession = _session == null && hasData;
                    var hideSession = _session != null && !hasData;

                    if (hideSession)
                    {
                        _session.Dismiss();
                    }
                    else if (showSession)
                    {
                        var syntaxTree = _model.SemanticModel.Compilation.SyntaxTree;
                        var snapshot = _document.GetTextSnapshot(syntaxTree);
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