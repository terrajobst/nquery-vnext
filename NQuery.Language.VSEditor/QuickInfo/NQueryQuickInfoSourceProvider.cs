using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    [Export(typeof (IQuickInfoSourceProvider))]
    [Name("NQueryQuickInfoSourceProvider")]
    [ContentType("NQuery")]
    internal sealed class NQueryQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var document = DocumentManager.GetDocument(textBuffer);
            return new NQueryQuickInfoSource();
        }
    }
}