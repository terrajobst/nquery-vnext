using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.Highlighting;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(HighlightTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQueryHighlightingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IHighlighterService HighlighterService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQueryHighlightingTagger(workspace, textView, HighlighterService.Highlighters) as ITagger<T>;
        }
    }
}