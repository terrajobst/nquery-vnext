using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("NQuery")]
    internal sealed class NQueryOutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var syntaxTreeManager = SyntaxTreeManagerService.GetSyntaxTreeManager(buffer);
            return new NQueryOutliningTagger(buffer, syntaxTreeManager) as ITagger<T>;
        }
    }
}