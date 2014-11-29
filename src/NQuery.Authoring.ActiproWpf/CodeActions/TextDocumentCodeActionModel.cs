using System;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;

using NQuery.Authoring.ActiproWpf.Text;
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
            var oldText = snapshot.ToSourceText();
            var syntaxTree = action.GetEdit();
            var newText = syntaxTree.Text;
            var changes = newText.GetChanges(oldText);

            var textChangeOptions = new TextChangeOptions();
            textChangeOptions.RetainSelection = true;

            var textChange = _document.CreateTextChange(TextChangeTypes.Custom, textChangeOptions);
            var currentText = oldText;

            foreach (var change in changes)
            {
                var range = currentText.ToRange(change.Span);
                textChange.ReplaceText(range, change.NewText);
                currentText = currentText.WithChanges(change);
            }

            textChange.Apply();
        }
    }
}