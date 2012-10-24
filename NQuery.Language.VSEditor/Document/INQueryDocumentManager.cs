using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor.Document
{
    public interface INQueryDocumentManager
    {
        INQueryDocument GetDocument(ITextBuffer textBuffer);
    }
}