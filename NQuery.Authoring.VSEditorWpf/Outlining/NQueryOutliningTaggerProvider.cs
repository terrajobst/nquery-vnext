using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryOutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryOutliningTagger(document) as ITagger<T>;
        }
    }
}