using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.Selection;
using NQuery.Authoring.Selection;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Selection
{
    internal sealed class NQuerySelectionProvider : INQuerySelectionProvider
    {
        private readonly ITextView _textView;
        private readonly ISelectionSpanProviderService _selectionSpanProviderService;
        private readonly Workspace _workspace;
        private readonly Stack<TextSpan> _selectionStack = new Stack<TextSpan>();

        public NQuerySelectionProvider(ITextView textView, ISelectionSpanProviderService selectionSpanProviderService)
        {
            _textView = textView;
            _selectionSpanProviderService = selectionSpanProviderService;
            _workspace = textView.TextBuffer.GetWorkspace();
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textView.Selection.SelectionChanged += SelectionOnSelectionChanged;
        }

        public async void ExtendSelection()
        {
            var documentView = _textView.GetDocumentView();
            var document = documentView.Document;
            var currentSelection = documentView.Selection;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var extendedSelection = syntaxTree.ExtendSelection(currentSelection, _selectionSpanProviderService.Providers);

            if (currentSelection == extendedSelection)
                return;

            _selectionStack.Push(currentSelection);
            Select(extendedSelection);
        }

        public void ShrinkSelection()
        {
            if (_selectionStack.Count == 0)
                return;

            var selection = _selectionStack.Pop();
            Select(selection);
        }

        private void Select(TextSpan textSpan)
        {
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, textSpan.Start, textSpan.Length);
            _textView.Selection.SelectionChanged -= SelectionOnSelectionChanged;
            _textView.Selection.Select(snapshotSpan, false);
            _textView.Caret.MoveTo(snapshotSpan.End);
            _textView.Selection.SelectionChanged += SelectionOnSelectionChanged;
        }

        private void ClearStack()
        {
            _selectionStack.Clear();
        }

        private void SelectionOnSelectionChanged(object sender, EventArgs e)
        {
            ClearStack();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            ClearStack();
        }
    }
}