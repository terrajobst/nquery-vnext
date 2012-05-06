using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryBraceTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var syntaxTreeManager = SyntaxTreeManagerService.GetCSharpSyntaxTreeManager(buffer);
            return new NQueryBraceTagger(textView, buffer, syntaxTreeManager) as ITagger<T>;
        }
    }
}