using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace NQuery.Authoring.VSEditorWpf.Commenting
{
    [Export(typeof(ICommentOperationsProvider))]
    internal sealed class CommentOperationsProvider : ICommentOperationsProvider
    {
        [Import]
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        public ICommentOperations GetCommentOperations(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var textBufferUndoManager = TextBufferUndoManagerProvider.GetTextBufferUndoManager(textView.TextBuffer);
                return new CommentOperations(textView, textBufferUndoManager);
            });
        }
    }
}