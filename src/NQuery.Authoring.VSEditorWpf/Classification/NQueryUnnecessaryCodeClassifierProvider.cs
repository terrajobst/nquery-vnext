using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryUnnecessaryCodeClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQueryClassificationService ClassificationService { get; set; }

        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public ICodeIssueService CodeIssueService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var semanticClassificationService = ClassificationService;
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryUnnecessaryCodeClassifier(semanticClassificationService, document, CodeIssueService) as ITagger<T>;
        }
    }
}