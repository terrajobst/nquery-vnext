using System;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

using NQuery.Authoring.Renaming;

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
            var renameModel = await RenameModel.CreateAsync(document, position);

            if (!renameModel.CanRename || renameModel.SymbolSpan == null)
                return;

            var renamedDocument = await renameModel.RenameAsync(renameModel.SymbolSpan.Value.Symbol.Name);

            ActiveSession = new RenameSession(this, _textBuffer, _textBufferUndoManager, renameModel, renamedDocument);
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