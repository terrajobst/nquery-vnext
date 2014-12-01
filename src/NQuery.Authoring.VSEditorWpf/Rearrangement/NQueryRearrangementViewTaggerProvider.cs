using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.VSEditorWpf.Classification;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryRearrangementViewTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IRearrangeModelManagerProvider RearrangeModelManagerProvider { get; set; }

        [Import]
        public INQueryClassificationService NQueryClassificationService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var rearrangeModelManager = RearrangeModelManagerProvider.GetRearrangeModelManager(textView);
            return new NQueryRearrangementViewTagger(buffer, rearrangeModelManager, NQueryClassificationService) as ITagger<T>;
        }
    }
}