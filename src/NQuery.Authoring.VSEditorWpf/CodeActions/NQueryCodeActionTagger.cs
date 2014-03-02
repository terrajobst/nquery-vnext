using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class NQueryCodeActionTagger : AsyncTagger<SmartTag, CodeActionSet>
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly ICodeIssueService _codeIssueService;
        private readonly ICodeRefactoringService _codeRefactoringService;

        private CodeIssue[] _codeIssues = new CodeIssue[0];

        public NQueryCodeActionTagger(ITextView textView, INQueryDocument document, ICodeIssueService codeIssueService, ICodeRefactoringService codeRefactoringService)
        {
            _textView = textView;
            _document = document;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            _codeIssueService = codeIssueService;
            _codeRefactoringService = codeRefactoringService;
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            InvalidateIssues();
            InvalidateTags();
        }

        private async void InvalidateIssues()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            _codeIssues = await Task.Run(() => _codeIssueService.GetIssues(semanticModel).ToArray());
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<CodeActionSet>>> GetRawTagsAsync()
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var position = _textView.Caret.Position.BufferPosition.Position;
            var semanticModel = await _document.GetSemanticModelAsync();
            var issueActions = await GetIssueActionsAsync(_codeIssues, position);
            var refactorings = await GetRefactoringsAsync(semanticModel, position);
            var codeActions = issueActions.Concat(refactorings);
            var codeActionSet = new CodeActionSet(position, codeActions);
            var codeActionSets = !codeActionSet.CodeActions.Any()
                ? Enumerable.Empty<CodeActionSet>()
                : new[] {codeActionSet};
            return Tuple.Create(snapshot, codeActionSets);
        }

        private static Task<IEnumerable<ICodeAction>> GetIssueActionsAsync(IEnumerable<CodeIssue> issues, int position)
        {
            return Task.Run(() => issues.Where(i => i.Span.ContainsOrTouches(position)).SelectMany(i => i.Actions));
        }

        private Task<IEnumerable<ICodeAction>> GetRefactoringsAsync(SemanticModel semanticModel, int position)
        {
            return Task.Run(() => _codeRefactoringService.GetRefactorings(semanticModel, position));
        }

        protected override ITagSpan<SmartTag> CreateTagSpan(ITextSnapshot snapshot, CodeActionSet rawTag)
        {
            var smartTagActions = new List<ISmartTagAction>();

            foreach (var codeAction in rawTag.CodeActions)
            {
                var smartTagAction = new SmarTagCodeAction(codeAction, _textView.TextBuffer);
                smartTagActions.Add(smartTagAction);
            }

            var smartTagActionSet = new SmartTagActionSet(smartTagActions.AsReadOnly());
            var smartTagActionSets = new ReadOnlyCollection<SmartTagActionSet>(new [] { smartTagActionSet });
            var smartTag = new SmartTag(SmartTagType.Factoid, smartTagActionSets);
            var span = new SnapshotSpan(snapshot, rawTag.Position, 1);
            return new TagSpan<SmartTag>(span, smartTag);
        }

        private sealed class SmarTagCodeAction : ISmartTagAction
        {
            private readonly ICodeAction _codeAction;
            private readonly ITextBuffer _textBuffer;

            public SmarTagCodeAction(ICodeAction codeAction, ITextBuffer textBuffer)
            {
                _codeAction = codeAction;
                _textBuffer = textBuffer;
            }

            public ReadOnlyCollection<SmartTagActionSet> ActionSets
            {
                get { return null; }
            }

            public ImageSource Icon
            {
                get { return null; }
            }

            public string DisplayText
            {
                get { return _codeAction.Description; }
            }

            public bool IsEnabled
            {
                get { return true;}
            }

            public void Invoke()
            {
                var snapshot = _textBuffer.CurrentSnapshot;
                var fullSpan = new Span(0, snapshot.Length);
                var syntaxTree = _codeAction.GetEdit();
                var newText = syntaxTree.TextBuffer.Text;
                _textBuffer.Replace(fullSpan, newText);
            }
        }
    }
}