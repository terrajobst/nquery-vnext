using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Wpf;

namespace NQuery.Authoring.VSEditorWpf.Completion
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