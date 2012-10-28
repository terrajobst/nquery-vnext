using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Selection
{
    internal sealed class NQuerySelectionProvider : INQuerySelectionProvider
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly Stack<TextSpan> _selectionStack = new Stack<TextSpan>();

        public NQuerySelectionProvider(ITextView textView, INQueryDocument document)
        {
            _textView = textView;
            _textView.Selection.SelectionChanged += SelectionOnSelectionChanged;
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
        }

        public async void ExtendSelection()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var currentSelection = GetCurrentSelection();
            var extendedSelection = ExtendSelection(syntaxTree, currentSelection);

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

        private TextSpan GetCurrentSelection()
        {
            var textSelection = _textView.Selection;
            var selectionStart = textSelection.Start.Position.Position;
            var selectionEnd = textSelection.End.Position;
            var currentSelection = TextSpan.FromBounds(selectionStart, selectionEnd);
            return currentSelection;
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

        private static TextSpan ExtendSelection(SyntaxTree syntaxTree, TextSpan selectedSpan)
        {
            var token = syntaxTree.Root.FindToken(selectedSpan.Start).GetPreviousTokenIfEndOfFile();
            if (!selectedSpan.Contains(token.Span))
                return token.Span;

            var node = token.Parent;
            while (node != null)
            {
                if (selectedSpan.Contains(node.Span))
                    node = node.Parent;
                else
                    return node.Span;
            }

            return syntaxTree.Root.Span;
        }

        private void SelectionOnSelectionChanged(object sender, EventArgs e)
        {
            ClearStack();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            ClearStack();
        }
    }
}