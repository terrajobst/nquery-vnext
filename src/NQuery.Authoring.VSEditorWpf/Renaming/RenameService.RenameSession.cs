using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;

using NQuery.Authoring.Renaming;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    partial class RenameService
    {
        private sealed class RenameSession : IRenameSession
        {
            private readonly RenameService _renameService;
            private readonly ITextBuffer _textBuffer;
            private readonly RenameModel _renameModel;

            private ITextSnapshot _renamedDocumentSnapshot;
            private ImmutableArray<SnapshotSpan> _locations; 
            private RenamedDocument _renamedDocument;

            public RenameSession(RenameService renameService, ITextBuffer textBuffer, RenameModel renameModel, RenamedDocument renamedDocument)
            {
                _renameService = renameService;
                _textBuffer = textBuffer;
                _renameModel = renameModel;
                _renamedDocument = renamedDocument;
                _renamedDocumentSnapshot = _textBuffer.CurrentSnapshot;

                _locations = ComputeLocations();

                Connect();
            }

            private void Connect()
            {
                _textBuffer.Changed += TextBufferOnChanged;
            }

            private void Disconnect()
            {
                _textBuffer.Changed -= TextBufferOnChanged;
                _renameService.ActiveSession = null;
            }

            private async void TextBufferOnChanged(object sender, TextContentChangedEventArgs e)
            {
                if (_textBuffer.CurrentSnapshot == _renamedDocumentSnapshot)
                    return;

                // 1. If the change isn't within any of the rename locations, then we'll commit
                //    the current rename session.
                //
                // 2. Otherwise, get the text at the merged location and use it to refresh
                //    the list of locations.

                if (e.Changes.Count == 0)
                    return;

                if (e.Changes.Count > 1)
                {
                    Commit();
                    return;
                }

                var change = e.Changes.Single();
                await ApplyChange(change);
            }

            private async Task ApplyChange(ITextChange change)
            {
                var newName = GetNewName(_renamedDocument, change);
                if (newName == null)
                {
                    Commit();
                    return;
                }

                var snapshot = _textBuffer.CurrentSnapshot;
                var renamedDocument = await _renameModel.RenameAsync(newName);

                if (_textBuffer.CurrentSnapshot != snapshot)
                    return;

                var newText = renamedDocument.Document.Text;
                var oldText = _renamedDocument.Document.Text;
                if (newText == oldText)
                    return;

                var changes = newText.GetChanges(oldText);

                using (var textEdit = _textBuffer.CreateEdit())
                {
                    var textSpanOfEvent = new TextSpan(change.OldSpan.Start, change.OldSpan.Length);

                    foreach (var textChange in changes)
                    {
                        if (!textChange.Span.IntersectsWith(textSpanOfEvent))
                            textEdit.Replace(textChange.Span.Start, textChange.Span.Length, textChange.NewText);
                    }

                    textEdit.Apply();
                }

                _renamedDocumentSnapshot = _textBuffer.CurrentSnapshot;
                _renamedDocument = renamedDocument;
                Locations = ComputeLocations();
            }

            private string GetNewName(RenamedDocument document, ITextChange change)
            {
                var oldSpan = new TextSpan(change.OldSpan.Start, change.OldSpan.Length);

                foreach (var location in document.Locations)
                {
                    if (location.IntersectsWith(oldSpan))
                    {
                        var newStart = Math.Min(oldSpan.Start, location.Start);
                        var newEnd = Math.Max(oldSpan.End, location.End) + change.Delta;
                        var newLength = newEnd - newStart;
                        return _textBuffer.CurrentSnapshot.GetText(newStart, newLength);
                    }
                }

                return null;
            }

            private ImmutableArray<SnapshotSpan> ComputeLocations()
            {
                var snapshot = _renamedDocumentSnapshot;
                return _renamedDocument.Locations.Select(s => new SnapshotSpan(snapshot, s.Start, s.Length))
                                                 .ToImmutableArray();
            }

            public void Commit()
            {
                // TODO: Cleanup undo stack
                Disconnect();
            }

            public void Cancel()
            {
                // TODO: Perform undo
                Disconnect();
            }

            private void OnLocationsChanged()
            {
                var handler = LocationsChanged;
                if (handler != null)
                    handler.Invoke(this, EventArgs.Empty);
            }

            public ImmutableArray<SnapshotSpan> Locations
            {
                get { return _locations; }
                private set
                {
                    if (_locations != value)
                    {
                        _locations = value;
                        OnLocationsChanged();
                    }
                }
            }

            public event EventHandler<EventArgs> LocationsChanged;
        }
    }
}