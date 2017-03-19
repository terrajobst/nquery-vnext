using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using NQuery.Authoring.Renaming;
using NQuery.Authoring.VSEditorWpf.Text;
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
            private readonly Document _oldDocument;
            private readonly ITextSnapshot _oldSnapshot;
            private readonly RenamedDocument _original;

            private ITextSnapshot _lastSnapshot;
            private ImmutableArray<SnapshotSpan> _locations;
            private string _current;

            public RenameSession(RenameService renameService, ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager, RenamedDocument renamedDocument)
            {
                _renameService = renameService;
                _textBuffer = textBuffer;
                _textBufferUndoManager = textBufferUndoManager;
                _oldDocument = textBuffer.GetDocument();
                _oldSnapshot = _textBuffer.CurrentSnapshot;
                _original = renamedDocument;

                _lastSnapshot = _oldSnapshot;
                _locations = ComputeLocations(_oldSnapshot, renamedDocument.Spans);

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
                var oldDocument = _oldDocument.WithText(_lastSnapshot.ToSourceText());
                var newDocument = _textBuffer.GetDocument();
                var current = await GetCurrent(oldDocument, newDocument);

                if (_textBuffer.CurrentSnapshot != newDocument.GetTextSnapshot())
                    return;

                if (current == null)
                {
                    Commit();
                    return;
                }

                _current = current;

                UndoLastRename();
                Rename();

                _lastSnapshot = _textBuffer.CurrentSnapshot;
            }

            private void Rename()
            {
                _textBuffer.PostChanged -= TextBufferOnChanged;

                using (var t = _textBufferUndoManager.TextBufferUndoHistory.CreateTransaction("Inline Rename"))
                {
                    foreach (var span in _original.Spans.OrderByDescending(s => s.Start))
                        _textBuffer.Replace(new Span(span.Start, span.Length), _current);

                    t.Complete();
                }

                _textBuffer.PostChanged += TextBufferOnChanged;

                Locations = ComputeLocations(_oldSnapshot, _original.Spans);
            }

            private ImmutableArray<SnapshotSpan> ComputeLocations(ITextSnapshot snapshot, ImmutableArray<TextSpan> spans)
            {
                return spans.Select(s => new SnapshotSpan(snapshot, s.Start, s.Length))
                            .Select(ss => ss.TranslateTo(_textBuffer.CurrentSnapshot, SpanTrackingMode.EdgeInclusive))
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
                _textBuffer.PostChanged -= TextBufferOnChanged;

                if (!_textBufferUndoManager.TextBufferUndoHistory.UndoStack.Any(IsRenameStart))
                    return;

                while (true)
                {
                    if (IsRenameStart(_textBufferUndoManager.TextBufferUndoHistory.UndoStack.FirstOrDefault()))
                        break;

                    _textBufferUndoManager.TextBufferUndoHistory.Undo(1);
                }

                var currentTransaction = _textBufferUndoManager.TextBufferUndoHistory.CurrentTransaction;
                if (currentTransaction != null &&
                    currentTransaction.State == UndoTransactionState.Open)
                {
                    currentTransaction.Cancel();
                    currentTransaction.Dispose();
                }

                if (undoStart)
                    _textBufferUndoManager.TextBufferUndoHistory.Undo(1);

                _textBuffer.PostChanged += TextBufferOnChanged;
            }

            private static bool IsRenameStart(ITextUndoTransaction textUndoTransaction)
            {
                return textUndoTransaction != null &&
                       textUndoTransaction.UndoPrimitives.Count == 1 &&
                       textUndoTransaction.UndoPrimitives[0] == RenameStartMarker.Instance;
            }

            private static async Task<string> GetCurrent(Document oldDocument, Document newDocument)
            {
                if (TryGetChange(oldDocument, newDocument, out var change))
                {
                    if (TryApplyIdentifierEdit(await oldDocument.GetSyntaxTreeAsync(), change, out var token, out var newIdentifierSpan, out var newIdentifer) ||
                        TryPostIdentifierEdit(await newDocument.GetSyntaxTreeAsync(), change, out token, out newIdentifierSpan, out newIdentifer))
                    {
                        return newIdentifer;
                    }
                }

                return null;
            }

            private static bool TryGetChange(Document oldDocument, Document newDocument, out TextChange change)
            {
                change = default(TextChange);

                var changes = newDocument.Text.GetChanges(oldDocument.Text).GetEnumerator();
                if (!changes.MoveNext())
                    return false;

                var first = changes.Current;

                if (changes.MoveNext())
                    return false;

                change = first;
                return true;
            }

            private static bool TryApplyIdentifierEdit(SyntaxTree syntaxTree, TextChange change, out SyntaxToken token, out TextSpan newSpan, out string newIdentifier)
            {
                token = null;
                newSpan = default(TextSpan);
                newIdentifier = null;

                token = FindOverlappingIdentifierOrKeyword(syntaxTree, change.Span);
                if (token == null)
                    return false;

                var delta = change.NewText.Length - change.Span.Length;

                var editStart = change.Span.Start;
                var start = Math.Min(editStart, token.Span.Start);
                var end = token.Span.End + delta;

                var newText = syntaxTree.Text.WithChanges(change);
                newSpan = TextSpan.FromBounds(start, end);
                newIdentifier = newText.GetText(newSpan);

                return newIdentifier.Length == 0 ||
                       SyntaxFacts.IsValidIdentifierOrKeyword(newIdentifier);
            }

            private static bool TryPostIdentifierEdit(SyntaxTree syntaxTree, TextChange change, out SyntaxToken token, out TextSpan newSpan, out string newIdentifier)
            {
                token = null;
                newSpan = default(TextSpan);
                newIdentifier = null;

                var span = new TextSpan(change.Span.Start, change.NewText.Length);

                token = FindOverlappingIdentifierOrKeyword(syntaxTree, span);
                if (token == null)
                    return false;

                newSpan = token.Span;
                newIdentifier = syntaxTree.Text.GetText(newSpan);

                return newIdentifier.Length == 0 ||
                       SyntaxFacts.IsValidIdentifierOrKeyword(newIdentifier);
            }

            private static SyntaxToken FindOverlappingIdentifierOrKeyword(SyntaxTree syntaxTree, TextSpan span)
            {
                var tokenAtStart = syntaxTree.Root.FindToken(span.Start);
                var tokenAtEnd = syntaxTree.Root.FindToken(span.End);
                var tokenBeforeEnd = tokenAtEnd.GetPreviousToken();

                // Case 1: Token contains span
                //
                //     [Span]
                // [----Token----]

                if (tokenAtStart == tokenAtEnd && tokenAtStart.Span.IntersectsWith(span) && tokenAtStart.Kind.IsIdentifierOrKeyword())
                    return tokenAtStart;

                // Case 2: Token starts at end
                //
                // [Span]
                //      [----Token----]

                if (span.End == tokenAtEnd.Span.Start && tokenAtEnd.Kind.IsIdentifierOrKeyword())
                    return tokenAtEnd;

                // Case 3: Beginning of token overlaps with span
                //
                // [Span]
                //    [----Token----]

                if (tokenAtEnd.Span.IntersectsWith(span) && tokenAtEnd.Kind.IsIdentifierOrKeyword())
                    return tokenAtEnd;

                // Case 4: Token ends at start
                //
                //               [Span]
                // [----Token----]

                if (tokenBeforeEnd != null && tokenBeforeEnd.Span.End == span.Start && tokenBeforeEnd.Kind.IsIdentifierOrKeyword())
                    return tokenBeforeEnd;

                // Case 5: End of token overlaps with span
                //
                //             [Span]
                // [----Token----]

                if (tokenAtStart.Span.IntersectsWith(span) && tokenAtStart.Kind.IsIdentifierOrKeyword())
                    return tokenAtStart;

                return null;
            }

            public void Commit()
            {
                UndoEntireSession();
                Rename();
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
