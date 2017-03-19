using System;
using Microsoft.VisualStudio.Text.Operations;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    partial class RenameService : IRenameService
    {
        private sealed class RenameStartMarker : ITextUndoPrimitive
        {
            public static readonly RenameStartMarker Instance = new RenameStartMarker();

            private RenameStartMarker()
            {
            }

            public void Do()
            {
            }

            public void Undo()
            {
            }

            public bool CanMerge(ITextUndoPrimitive older)
            {
                return false;
            }

            public ITextUndoPrimitive Merge(ITextUndoPrimitive older)
            {
                throw new InvalidOperationException();
            }

            public ITextUndoTransaction Parent { get; set; }
            public bool CanRedo { get { return false; } }
            public bool CanUndo { get { return true; } }
        }
    }
}
