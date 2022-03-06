using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

using NQuery.Authoring.Commenting;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Commenting
{
    internal sealed class CommentOperations : ICommentOperations
    {
        private readonly ITextView _textView;
        private readonly ITextBufferUndoManager _textBufferUndoManager;

        public CommentOperations(ITextView textView, ITextBufferUndoManager textBufferUndoManager)
        {
            _textView = textView;
            _textBufferUndoManager = textBufferUndoManager;
        }

        public async void ToggleSingleLineComment()
        {
            await ToggleComment(Commenter.ToggleSingleLineComment);
        }

        public async void ToggleMultiLineComment()
        {
            await ToggleComment(Commenter.ToggleMultiLineComment);
        }

        private async Task ToggleComment(Func<SyntaxTree, TextSpan, SyntaxTree> commenter)
        {
            var workspace = _textView.TextBuffer.GetWorkspace();
            var documentView = _textView.GetDocumentView();

            var syntaxTree = await documentView.Document.GetSyntaxTreeAsync();
            if (documentView.Document != workspace.CurrentDocument)
                return;

            var newSyntaxTree = commenter(syntaxTree, documentView.Selection);

            var oldText = documentView.Document.Text;
            var newText = newSyntaxTree.Text;
            var changes = newText.GetChanges(oldText);

            using var transaction = _textBufferUndoManager.TextBufferUndoHistory.CreateTransaction(Resources.TransactionToggleComment);

            foreach (var change in changes)
            {
                var textSpan = change.Span;
                var span = new Span(textSpan.Start, textSpan.Length);
                var text = change.NewText;
                _textView.TextBuffer.Replace(span, text);
            }

            transaction.Complete();
        }
    }
}