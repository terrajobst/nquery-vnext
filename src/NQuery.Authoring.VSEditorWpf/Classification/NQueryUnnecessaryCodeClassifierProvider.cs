using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQueryUnnecessaryCodeClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQueryClassificationService ClassificationService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var semanticClassificationService = ClassificationService;
            var workspace = buffer.GetWorkspace();
            return new NQueryUnnecessaryCodeClassifier(semanticClassificationService, workspace) as ITagger<T>;
        }
    }
}