using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Commenting
{
    public interface ICommentOperationsProvider
    {
        ICommentOperations GetCommentOperations(ITextView textView);
    }
}