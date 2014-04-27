using System;

using Microsoft.VisualStudio.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.VSEditorWpf.Document
{
    public interface INQueryDocumentManager
    {
        NQueryDocument GetDocument(ITextBuffer textBuffer);
    }
}