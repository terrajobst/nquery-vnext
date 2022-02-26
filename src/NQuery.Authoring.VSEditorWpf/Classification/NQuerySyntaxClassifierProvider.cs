using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    [Export(typeof (ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(@"NQuery")]
    [Name(@"NQuerySyntaxClassifier")]
    internal sealed class NQuerySyntaxClassifierProvider : ITaggerProvider
    {
        [Import]
        public INQueryClassificationService ClassificationService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQuerySyntaxClassifier(ClassificationService, workspace) as ITagger<T>;
        }
    }
}