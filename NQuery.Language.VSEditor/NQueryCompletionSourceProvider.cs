using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("NQueryCompletionSourceProvider")]
    [ContentType("NQuery")]
    internal sealed class NQueryCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        [Import]
        public INQueryGlyphService GlyphService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(textBuffer);
            var glyphService = GlyphService;
            return new NQueryCompletionSource(semanticModelManager, glyphService);
        }
    }
}