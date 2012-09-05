using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("NQuery")]
    internal sealed class NQuerySyntaxErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var semanticModelManager = SyntaxTreeManagerService.GetSyntaxTreeManager(buffer);
            return new NQuerySyntaxErrorTagger(buffer, semanticModelManager) as ITagger<T>;
        }
    }
}