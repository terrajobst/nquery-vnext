using System;

namespace NQuery.Authoring.VSEditorWpf.Commenting
{
    public interface ICommentOperations
    {
        void ToggleSingleLineComment();
        void ToggleMultiLineComment();
    }
}