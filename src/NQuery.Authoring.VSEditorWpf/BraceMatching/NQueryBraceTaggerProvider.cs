using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.BraceMatching;

namespace NQuery.Authoring.VSEditorWpf.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQueryBraceTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IBraceMatcherService BraceMatcherService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQueryBraceTagger(workspace, textView, BraceMatcherService) as ITagger<T>;
        }
    }
}