using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQuerySemanticErrorTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQuerySemanticErrorTagger(workspace) as ITagger<T>;
        }
    }
}