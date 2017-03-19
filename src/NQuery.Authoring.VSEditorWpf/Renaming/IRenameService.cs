
using System;
using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    public interface IRenameService
    {
        void StartSession(SnapshotPoint point);

        IRenameSession ActiveSession { get; }

        event EventHandler<EventArgs> ActiveSessionChanged;
    }
}
