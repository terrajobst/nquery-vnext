using System;

using Microsoft.VisualStudio.Text;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class TextBufferCodeActionModel : CodeActionModel
    {
        private readonly ITextBuffer _textBuffer;

        public TextBufferCodeActionModel(CodeActionKind kind, ICodeAction codeAction, ITextBuffer textBuffer)
            : base(kind, codeAction)
        {
            _textBuffer = textBuffer;
        }

        protected override void Invoke(ICodeAction action)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var fullSpan = new Span(0, snapshot.Length);
            var syntaxTree = action.GetEdit();
            var newText = syntaxTree.Text.GetText();
            _textBuffer.Replace(fullSpan, newText);
        }
    }
}