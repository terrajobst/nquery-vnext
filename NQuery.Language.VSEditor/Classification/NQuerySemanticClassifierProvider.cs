using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    internal sealed class NQuerySemanticClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQuerySemanticClassificationService SemanticClassificationService { get; set; }

        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var semanticClassificationService = SemanticClassificationService;
            var document = DocumentManager.GetDocument(buffer);
            return new NQuerySemanticClassifier(semanticClassificationService, document) as ITagger<T>;
        }
    }
}