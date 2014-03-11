using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryCodeIssueTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public ICodeIssueProviderService CodeIssueProviderService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryCodeIssueTagger(document, CodeIssueProviderService) as ITagger<T>;
        }
    }
}