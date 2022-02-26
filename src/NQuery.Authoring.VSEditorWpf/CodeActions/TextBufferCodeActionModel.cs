using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.VSEditorWpf.Text;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class TextBufferCodeActionModel : CodeActionModel
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextBufferUndoManager _textBufferUndoManager;

        public TextBufferCodeActionModel(CodeActionKind kind, ICodeAction codeAction, ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager)
            : base(kind, codeAction)
        {
            _textBuffer = textBuffer;
            _textBufferUndoManager = textBufferUndoManager;
        }

        protected override void Invoke(ICodeAction action)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var oldText = snapshot.ToSourceText();
            var syntaxTree = action.GetEdit();
            var newText = syntaxTree.Text;
            var changes = newText.GetChanges(oldText);

            using (var t = _textBufferUndoManager.TextBufferUndoHistory.CreateTransaction(action.Description))
            {
                foreach (var change in changes)
                {
                    var textSpan = change.Span;
                    var span = new Span(textSpan.Start, textSpan.Length);
                    var text = change.NewText;
                    _textBuffer.Replace(span, text);
                }

                t.Complete();
            }
        }
    }
}