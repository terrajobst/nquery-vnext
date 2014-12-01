using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    [Export(typeof(ISyntaxTreeApplierProvider))]
    internal sealed class SyntaxTreeApplierProvider : ISyntaxTreeApplierProvider
    {
        [Import]
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        public ISyntaxTreeApplier GetSyntaxTreeApplier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var textBufferUndoManager = TextBufferUndoManagerProvider.GetTextBufferUndoManager(textBuffer);
                return new SyntaxTreeApplier(textBuffer, textBufferUndoManager);
            });
        }
    }
}