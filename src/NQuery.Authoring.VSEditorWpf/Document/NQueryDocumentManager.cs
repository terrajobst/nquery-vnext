using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

using NQuery.Authoring.Document;
using NQuery.Authoring.VSEditorWpf.Text;

namespace NQuery.Authoring.VSEditorWpf.Document
{
    [Export(typeof(INQueryDocumentManager))]
    internal sealed class NQueryDocumentManager : INQueryDocumentManager
    {
        public NQueryDocument GetDocument(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var textBufferProvider = new SnapshotTextBufferPovider(textBuffer);
                return new NQueryDocument(textBufferProvider);
            });
        }
    }
}