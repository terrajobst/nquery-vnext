using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Completion
{
    internal sealed class CompletionModelManager : ICompletionModelManager
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly ICompletionBroker _completionBroker;
        private readonly IEnumerable<ICompletionProvider> _completionItemProviders;

        private ICompletionSession _session;
        private CompletionModel _model;

        public CompletionModelManager(ITextView textView, INQueryDocument document, ICompletionBroker completionBroker, IEnumerable<ICompletionProvider> completionItemProviders)
        {
            _completionBroker = completionBroker;
            _completionItemProviders = completionItemProviders;
            _textView = textView;
            _document = document;
            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
        }

        private static bool IsTriggerChar(char c)
        {
            return char.IsLetter(c) ||
                   c == '_' ||
                   c == '.';
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

        private void TextBufferOnPostChanged(object sender, EventArgs e)
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
            var textView = _textView;
            var triggerPosition = textView.Caret.Position.BufferPosition;
            var semanticModel = await _document.GetSemanticModelAsync();

            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, triggerPosition);
            var applicableSpan = token == null ? new TextSpan(triggerPosition, 0) : token.Value.Span;

            var items = _completionItemProviders.SelectMany(p => p.GetItems(semanticModel, applicableSpan.Start)).ToArray();

            // Let observers know that we've a new model.

            Model = new CompletionModel(semanticModel, applicableSpan, items);
        }

        private static SyntaxToken? GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenTouched(position, descendIntoTrivia: true);
            return syntaxToken.Kind.IsIdentifierOrKeyword()
                       ? syntaxToken
                       : (SyntaxToken?)null;
        }

        public bool Commit()
        {
            if (_session == null)
                return false;

            _textView.TextBuffer.PostChanged -= TextBufferOnPostChanged;
            _session.Commit();
            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
            return true;
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

                var hasData = _model != null && _model.Items.Count > 0;
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