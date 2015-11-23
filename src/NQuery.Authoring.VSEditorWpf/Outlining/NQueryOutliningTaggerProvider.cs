using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.Outlining;

namespace NQuery.Authoring.VSEditorWpf.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQueryOutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        public IOutliningService OutliningService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQueryOutliningTagger(workspace, OutliningService) as ITagger<T>;
        }
    }
}