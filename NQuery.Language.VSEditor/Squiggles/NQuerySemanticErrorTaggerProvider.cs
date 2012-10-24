using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("NQuery")]
    internal sealed class NQuerySemanticErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQuerySemanticErrorTagger(document) as ITagger<T>;
        }
    }
}