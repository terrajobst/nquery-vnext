using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using NQuery.Authoring.Renaming;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    partial class RenameService : IRenameService
    {
        private sealed class RenameSession : IRenameSession
        {
            private readonly RenameService _renameService;
            private readonly ITextBuffer _textBuffer;
            private readonly ITextBufferUndoManager _textBufferUndoManager;

            private ITextSnapshot _renamedDocumentSnapshot;
            private ImmutableArray<SnapshotSpan> _locations;
            private RenamedDocument _renamedDocument;

            public RenameSession(RenameService renameService, ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager, RenamedDocument renamedDocument)
            {
                _renameService = renameService;
                _textBuffer = textBuffer;
                _textBufferUndoManager = textBufferUndoManager;
                _renamedDocument = renamedDocument;
                _renamedDocumentSnapshot = _textBuffer.CurrentSnapshot;

                _locations = ComputeLocations();

                Connect();
                MarkBeginningOfUndo();
            }

            private void Connect()
            {
                _textBuffer.PostChanged += TextBufferOnChanged;
            }

            private void Disconnect()
            {
                _textBuffer.PostChanged -= TextBufferOnChanged;
                _renameService.ActiveSession = null;
            }

            private async void TextBufferOnChanged(object sender, EventArgs e)
            {
                if (_textBufferUndoManager.TextBufferUndoHistory.State != TextUndoHistoryState.Idle)
                    return;

                if (_textBuffer.CurrentSnapshot == _renamedDocumentSnapshot)
                    return;

                // 1. If the change isn't within any of the rename locations, then we'll commit
                //    the current rename session.
                //
                // 2. Otherwise, get the text at the merged location and use it to refresh
                //    the list of locations.

                var changes = _renamedDocumentSnapshot.Version.Changes;
                if (changes.Count == 0)
                    return;

                if (changes.Count > 1)
                {
                    Commit();
                    return;
                }

                var change = changes.Single();

                var span = new TextSpan(change.OldPosition, change.OldLength);
                var textChange = new TextChange(span, change.NewText);

                _textBuffer.PostChanged -= TextBufferOnChanged;

                await ApplyChange(textChange);

                _textBuffer.PostChanged += TextBufferOnChanged;
            }

            private async Task ApplyChange(TextChange change)
            {
                _renamedDocument = await _renamedDocument.ApplyAsync(change);
                if (!_renamedDocument.IsRenamed)
                {
                    Commit();
                    return;
                }

                var snapshot = _textBuffer.CurrentSnapshot;
                if (_textBuffer.CurrentSnapshot != snapshot)
                    return;

                UndoLastRename();

                var changes = _renamedDocument.Changes;

                using (var textEdit = _textBuffer.CreateEdit())
                {
                    foreach (var newChange in _renamedDocument.Changes)
                        textEdit.Replace(newChange.Span.Start, newChange.Span.Length, newChange.NewText);

                    textEdit.Apply();
                }

                _renamedDocumentSnapshot = _textBuffer.CurrentSnapshot;
                Locations = ComputeLocations();
            }

            private ImmutableArray<SnapshotSpan> ComputeLocations()
            {
                var snapshot = _renamedDocumentSnapshot;
                return _renamedDocument.NewSpans.Select(s => new SnapshotSpan(snapshot, s.Start, s.Length))
                                                .ToImmutableArray();
            }

            private void MarkBeginningOfUndo()
            {
                using (var textUndoTransaction = _textBufferUndoManager.TextBufferUndoHistory.CreateTransaction("Rename start"))
                {
                    textUndoTransaction.AddUndo(RenameStartMarker.Instance);
                    textUndoTransaction.Complete();
                }
            }

            private void UndoEntireSession()
            {
                PerformUndo(true);
            }

            private void UndoLastRename()
            {
                PerformUndo(false);
            }

            private void PerformUndo(bool undoStart)
            {
                if (!_textBufferUndoManager.TextBufferUndoHistory.UndoStack.Any(IsRenameStart))
                    return;

                while (true)
                {
                    if (IsRenameStart(_textBufferUndoManager.TextBufferUndoHistory.UndoStack.FirstOrDefault()))
                        break;

                    _textBufferUndoManager.TextBufferUndoHistory.Undo(1);
                }

                if (undoStart)
                    _textBufferUndoManager.TextBufferUndoHistory.Undo(1);
            }

            private static bool IsRenameStart(ITextUndoTransaction textUndoTransaction)
            {
                return textUndoTransaction != null &&
                       textUndoTransaction.UndoPrimitives.Count == 1 &&
                       textUndoTransaction.UndoPrimitives[0] == RenameStartMarker.Instance;
            }

            public void Commit()
            {
                // TODO: Replace current undo stack with "Rename"
                UndoEntireSession();
                Disconnect();
            }

            public void Cancel()
            {
                UndoEntireSession();
                Disconnect();
            }

            private void OnLocationsChanged()
            {
                LocationsChanged?.Invoke(this, EventArgs.Empty);
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
