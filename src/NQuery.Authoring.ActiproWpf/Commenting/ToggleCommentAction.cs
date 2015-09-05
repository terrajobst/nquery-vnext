using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Commenting
{
    public abstract class ToggleCommentAction : EditActionBase
    {
        protected abstract SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan textSpan);

        public override async void Execute(IEditorView view)
        {
            var oldSelection = view.Selection.SnapshotRange;
            var documentView = view.SyntaxEditor.GetDocumentView();

            var editorDocument = view.SyntaxEditor.Document;
            var snapshot = editorDocument.CurrentSnapshot;
            var syntaxTree = await documentView.Document.GetSyntaxTreeAsync();

            if (editorDocument.CurrentSnapshot != snapshot)
                return;

            var newSyntaxTree = ToggleComment(syntaxTree, documentView.Selection);
            var oldText = documentView.Document.Text;
            var newText = newSyntaxTree.Text;
            var changes = newText.GetChanges(oldText);

            var textChange = editorDocument.CreateTextChange(TextChangeTypes.CommentLines);

            foreach (var change in changes)
                textChange.ReplaceText(oldText.ToRange(change.Span), change.NewText);

            textChange.Apply();

            var newSelection = oldSelection.TranslateTo(editorDocument.CurrentSnapshot, TextRangeTrackingModes.Default);
            view.Selection.SelectRange(newSelection);
        }
    }
}