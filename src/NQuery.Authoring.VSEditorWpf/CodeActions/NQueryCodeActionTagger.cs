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
using NQuery.Authoring.Composition.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class NQueryCodeActionTagger : AsyncTagger<SmartTag, CodeActionSet>
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly ICodeIssueProviderService _codeIssueProviderService;
        private readonly ICodeRefactoringProviderService _codeRefactoringProviderService;

        private CodeIssue[] _codeIssues = new CodeIssue[0];

        public NQueryCodeActionTagger(ITextView textView, INQueryDocument document, ICodeIssueProviderService codeIssueProviderService, ICodeRefactoringProviderService codeRefactoringProviderService)
        {
            _textView = textView;
            _document = document;
            _codeIssueProviderService = codeIssueProviderService;
            _codeRefactoringProviderService = codeRefactoringProviderService;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
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
            var providers = _codeIssueProviderService.Providers;
            _codeIssues = await Task.Run(() => semanticModel.GetIssues(providers).ToArray());
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<CodeActionSet>>> GetRawTagsAsync()
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var position = _textView.Caret.Position.BufferPosition.Position;
            var semanticModel = await _document.GetSemanticModelAsync();
            var issueActions = await GetIssueActionsAsync(_codeIssues, position);
            var refactorings = await GetRefactoringsAsync(semanticModel, position, _codeRefactoringProviderService.Providers);
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

        private static Task<IEnumerable<ICodeAction>> GetRefactoringsAsync(SemanticModel semanticModel, int position, IEnumerable<ICodeRefactoringProvider> providers)
        {
            return Task.Run(() => semanticModel.GetRefactorings(position, providers));
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