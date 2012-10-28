using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Document
{
    [Export(typeof(INQueryDocumentManager))]
    internal sealed class NQueryDocumentManager : INQueryDocumentManager
    {
        public INQueryDocument GetDocument(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new NQueryDocument(textBuffer));
        }
    }
}