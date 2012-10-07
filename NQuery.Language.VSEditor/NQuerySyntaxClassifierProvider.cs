using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    [Export(typeof (ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    [Name("NQuerySyntaxClassifier")]
    internal sealed class NQuerySyntaxClassifierProvider : ITaggerProvider
    {
        [Import]
        public IStandardClassificationService ClassificationService { get; set; }

        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQuerySyntaxClassifier(ClassificationService, document) as ITagger<T>;
        }
    }
}