using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("NQueryCompletionSourceProvider")]
    [ContentType("NQuery")]
    internal sealed class NQueryCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        public INQueryGlyphService GlyphService { get; set; }

        [Import]
        public ICompletionModelManagerProvider CompletionModelManagerProvider  { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var glyphService = GlyphService;
            return new NQueryCompletionSource(glyphService);
        }
    }
}