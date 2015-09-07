using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    public interface IRenameServiceProvider
    {
        IRenameService GetRenameService(ITextBuffer textBuffer);
    }
}