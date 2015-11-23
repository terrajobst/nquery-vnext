using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(@"NQuery")]
    internal sealed class NQueryCodeIssueTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public ICodeIssueProviderService CodeIssueProviderService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var workspace = buffer.GetWorkspace();
            return new NQueryCodeIssueTagger(workspace, CodeIssueProviderService) as ITagger<T>;
        }
    }
}