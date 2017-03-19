using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using NQuery.Authoring.Renaming;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    internal sealed partial class RenameService : IRenameService
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextBufferUndoManager _textBufferUndoManager;

        private IRenameSession _activeSession;

        public RenameService(ITextBuffer textBuffer, ITextBufferUndoManager textBufferUndoManager)
        {
            _textBuffer = textBuffer;
            _textBufferUndoManager = textBufferUndoManager;
        }

        public async void StartSession(SnapshotPoint point)
        {
            if (ActiveSession != null)
                ActiveSession.Commit();

            var workspace = _textBuffer.GetWorkspace();
            var document = workspace.CurrentDocument;
            var position = point.TranslateTo(document.GetTextSnapshot(), PointTrackingMode.Negative).Position;

            var syntaxTree = await document.GetSyntaxTreeAsync();
            var token = syntaxTree.Root.FindToken(position);
            var change = TextChange.ForReplacement(token.Span, token.Text);

            var renamedDocument = await RenamedDocument.CreateAsync(document, change);

            if (!renamedDocument.IsRenamed)
                return;

            ActiveSession = new RenameSession(this, _textBuffer, _textBufferUndoManager, renamedDocument);
        }

        private void OnActiveSessionChanged()
        {
            var handler = ActiveSessionChanged;
            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        public IRenameSession ActiveSession
        {
            get { return _activeSession; }
            private set
            {
                if (_activeSession != value)
                {
                    _activeSession = value;
                    OnActiveSessionChanged();
                }
            }
        }

        public event EventHandler<EventArgs> ActiveSessionChanged;
    }
}
