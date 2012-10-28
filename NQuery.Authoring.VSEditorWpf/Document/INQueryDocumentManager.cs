using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Document
{
    public interface INQueryDocumentManager
    {
        INQueryDocument GetDocument(ITextBuffer textBuffer);
    }
}