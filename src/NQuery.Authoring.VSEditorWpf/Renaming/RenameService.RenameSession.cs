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
    partial class RenameService
    {
        private sealed class RenameSession : IRenameSession
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
                public bool CanRedo { get { return true; } }
                public bool CanUndo { get { return true; } }
            }

            private readonly RenameService _renameService;
            private readonly ITextBuffer _textBuffer;
            private readonly ITextBufferUndoManager _textBufferUndoManager;
            private readonly RenameModel _renameModel;

            private ITextSnapshot _renamedDocumentSnapshot;
            private ImmutableArray<SnapshotSpan> _locations; 
            private RenamedDocument _renamedDocument;

            public RenameSession(RenameService renameService, ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager, RenameModel renameModel, RenamedDocument renamedDocument)
            {
                _renameService = renameService;
                _textBuffer = textBuffer;
                _textBufferUndoManager = textBufferUndoManager;
                _renameModel = renameModel;
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

            private async void TextBufferOnChanged(object sender, EventArgs eventArgs)
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

                UndoLastRename();

                var changes = renamedDocument.Changes;

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
                        var newEnd = Math.Max(oldSpan.End, location.End);
                        var newLength = newEnd - newStart + change.Delta;
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