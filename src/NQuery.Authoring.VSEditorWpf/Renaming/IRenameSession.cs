using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    public interface IRenameSession
    {
        void Commit();
        void Cancel();

        ImmutableArray<SnapshotSpan> Locations { get; }

        event EventHandler<EventArgs> LocationsChanged;
    }
}
