using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    [Export(typeof(IRenameServiceProvider))]
    internal sealed class RenameServiceProvider : IRenameServiceProvider
    {
        [Import]
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        public IRenameService GetRenameService(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var textBufferUndoManager = TextBufferUndoManagerProvider.GetTextBufferUndoManager(textBuffer);
                return new RenameService(textBuffer, textBufferUndoManager);
            });
        }
    }
}
