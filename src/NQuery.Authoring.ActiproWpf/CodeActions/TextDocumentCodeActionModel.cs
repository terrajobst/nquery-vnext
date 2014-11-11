using System;

using ActiproSoftware.Text;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    internal sealed class TextDocumentCodeActionModel : CodeActionModel
    {
        private readonly ITextDocument _document;

        public TextDocumentCodeActionModel(CodeActionKind kind, ICodeAction codeAction, ITextDocument document)
            : base(kind, codeAction)
        {
            _document = document;
        }

        protected override void Invoke(ICodeAction action)
        {
            var snapshot = _document.CurrentSnapshot;
            var fullSpan = new TextRange(0, snapshot.Length);
            var syntaxTree = action.GetEdit();
            var newText = syntaxTree.Text.GetText();
            _document.ReplaceText(TextChangeTypes.Custom, fullSpan, newText);
        }
    }
}