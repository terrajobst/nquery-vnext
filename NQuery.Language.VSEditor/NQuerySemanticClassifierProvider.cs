using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    internal sealed class NQuerySemanticClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQuerySemanticClassificationService SemanticClassificationService { get; set; }

        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var semanticClassificationService = SemanticClassificationService;
            var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(buffer);
            return new NQuerySemanticClassifier(semanticClassificationService, buffer, semanticModelManager) as ITagger<T>;
        }
    }
}