using System;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Completion;
using NQuery.Authoring.Composition.Completion;
using NQuery.Authoring.VSEditorWpf.Text;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    internal sealed class CompletionModelManager : ICompletionModelManager
    {
        private readonly Workspace _workspace;
        private readonly ITextView _textView;
        private readonly ICompletionBroker _completionBroker;
        private readonly ICompletionProviderService _completionProviderService;

        private ICompletionSession _session;
        private CompletionModel _model;

        public CompletionModelManager(Workspace workspace, ITextView textView, ICompletionBroker completionBroker, ICompletionProviderService completionProviderService)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textView = textView;
            _completionBroker = completionBroker;
            _completionProviderService = completionProviderService;
        }

        private static bool IsTriggerChar(char c)
        {
            return char.IsLetter(c) ||
                   c == '_' ||
                   c == '.' ||
                   c == '\'';
        }

        private static bool IsCommitChar(char c)
        {
            return char.IsWhiteSpace(c) ||
                   c == '\t' ||
                   c == '.' ||
                   c == ',' ||
                   c == '(' ||
                   c == ')';
        }

        public void HandleTextInput(string text)
        {
            if (_session == null && text.Any(IsTriggerChar))
            {
                TriggerCompletion(false);
            }
            else if (_session != null)
            {
                UpdateModel();
            }
        }

        public void HandlePreviewTextInput(string text)
        {
            if (_session != null && text.Any(IsCommitChar))
                Commit();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            if (_session != null)
                UpdateModel();
        }

        public void TriggerCompletion(bool autoComplete)
        {
            if (_session != null)
                _session.Dismiss();
            else
                UpdateModel();
        }

        private async void UpdateModel()
        {
            var documentView = _textView.GetDocumentView();
            var position = documentView.Position;
            var document = documentView.Document;
            var semanticModel = await document.GetSemanticModelAsync();
            var model = semanticModel.GetCompletionModel(position, _completionProviderService.Providers);

            // Let observers know that we've a new model.

            Model = model;
        }

        public bool Commit()
        {
            if (_session == null)
                return false;

            var isBuilder = _session.SelectedCompletionSet != null &&
                            _session.SelectedCompletionSet.SelectionStatus != null &&
                            _session.SelectedCompletionSet.SelectionStatus.Completion != null &&
                            _session.SelectedCompletionSet.CompletionBuilders != null &&
                            _session.SelectedCompletionSet.CompletionBuilders.Contains(_session.SelectedCompletionSet.SelectionStatus.Completion);

            // If it's a builder, we don't want to eat the enter key.
            var sendThrough = isBuilder;

            _workspace.CurrentDocumentChanged -= WorkspaceOnCurrentDocumentChanged;
            _session.Commit();
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;

            return !sendThrough;
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
        }

        private void OnModelChanged(EventArgs e)
        {
            var handler = ModelChanged;
            if (handler != null)
                handler(this, e);
        }

        public CompletionModel Model
        {
            get { return _model; }
            private set
            {
                _model = value;
                OnModelChanged(EventArgs.Empty);

                var hasData = _model != null && _model.Items.Length > 0;
                var showSession = _session == null && hasData;
                var hideSession = _session != null && !hasData;

                if (hideSession)
                {
                    _session.Dismiss();
                }
                else if (showSession)
                {
                    var syntaxTree = _model.SemanticModel.SyntaxTree;
                    var snapshot = syntaxTree.Text.ToTextSnapshot();
                    var triggerPosition = _model.ApplicableSpan.Start;
                    var triggerPoint = snapshot.CreateTrackingPoint(triggerPosition, PointTrackingMode.Negative);

                    _session = _completionBroker.CreateCompletionSession(_textView, triggerPoint, true);
                    _session.Properties.AddProperty(typeof(ICompletionModelManager), this);
                    _session.Dismissed += SessionOnDismissed;
                    _session.Start();
                }
            }
        }

        public event EventHandler<EventArgs> ModelChanged;
    }
}