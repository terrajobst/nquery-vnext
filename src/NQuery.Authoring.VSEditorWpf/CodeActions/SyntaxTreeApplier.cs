using System;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

using NQuery.Authoring.VSEditorWpf.Text;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class SyntaxTreeApplier : ISyntaxTreeApplier
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextBufferUndoManager _textBufferUndoManager;

        public SyntaxTreeApplier(ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager)
        {
            _textBuffer = textBuffer;
            _textBufferUndoManager = textBufferUndoManager;
        }

        public void Apply(SyntaxTree syntaxTree, string description)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var oldText = snapshot.ToSourceText();
            var newText = syntaxTree.Text;
            var changes = newText.GetChanges(oldText);

            using (var t = _textBufferUndoManager.TextBufferUndoHistory.CreateTransaction(description))
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