using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryBraceTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public IBraceMatchingService BraceMatchingService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryBraceTagger(textView, buffer, document, BraceMatchingService) as ITagger<T>;
        }
    }
}