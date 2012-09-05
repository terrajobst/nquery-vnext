using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySelectionProvider : INQuerySelectionProvider
    {
        private readonly ITextView _textView;
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;
        private readonly Stack<TextSpan> _selectionStack = new Stack<TextSpan>();

        public NQuerySelectionProvider(ITextView textView, INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _textView = textView;
            _textView.Selection.SelectionChanged += SelectionOnSelectionChanged;
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        public bool ExtendSelection()
        {
            if (_syntaxTreeManager.SyntaxTree == null)
                return false;

            var currentSelection = GetCurrentSelection();
            var extendedelection = ExtendSelection(_syntaxTreeManager.SyntaxTree, currentSelection);

            if (currentSelection == extendedelection)
                return false;

            _selectionStack.Push(currentSelection);
            Select(extendedelection);
            return true;
        }

        public bool ShrinkSelection()
        {
            if (_selectionStack.Count == 0)
                return false;

            var selection = _selectionStack.Pop();
            Select(selection);
            return true;
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
            _textView.Selection.SelectionChanged += SelectionOnSelectionChanged;
        }

        private void ClearStack()
        {
            _selectionStack.Clear();
        }

        private static TextSpan ExtendSelection(SyntaxTree syntaxTree, TextSpan selectedSpan)
        {
            var token = syntaxTree.Root.FindToken(selectedSpan.Start);
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

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            ClearStack();
        }
    }
}