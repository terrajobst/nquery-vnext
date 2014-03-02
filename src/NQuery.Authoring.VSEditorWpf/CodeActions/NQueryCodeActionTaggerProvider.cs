using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(SmartTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryCodeActionTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public ICodeIssueService CodeIssueService { get; set; }

        [Import]
        public ICodeRefactoringService CodeRefactoringService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var document = DocumentManager.GetDocument(buffer);
            return new NQueryCodeActionTagger(textView, document, CodeIssueService, CodeRefactoringService) as ITagger<T>;
        }
    }
}