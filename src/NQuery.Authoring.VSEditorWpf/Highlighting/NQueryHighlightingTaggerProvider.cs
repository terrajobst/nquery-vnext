using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.Highlighting;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(HighlightTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryHighlightingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public IHighlighterService HighlighterService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryHighlightingTagger(textView, document, HighlighterService.Highlighters) as ITagger<T>;
        }
    }
}