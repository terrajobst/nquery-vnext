using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using NQuery.Authoring.VSEditorWpf.Classification;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    internal sealed class RenameMarkerClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQueryClassificationService ClassificationService { get; set; }

        [Import]
        public IRenameServiceProvider RenameServiceProvider { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var semanticClassificationService = ClassificationService;
            var renameService = RenameServiceProvider.GetRenameService(buffer);
            return new RenameMarkerClassifier(buffer, semanticClassificationService, renameService) as ITagger<T>;
        }
    }
}
