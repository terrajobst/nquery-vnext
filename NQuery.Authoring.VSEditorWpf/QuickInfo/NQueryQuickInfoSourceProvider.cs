using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Classification;
using NQuery.Language.Wpf;

namespace NQuery.Language.VSEditor.QuickInfo
{
    [Export(typeof (IQuickInfoSourceProvider))]
    [Name("NQueryQuickInfoSourceProvider")]
    [ContentType("NQuery")]
    internal sealed class NQueryQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        public INQueryGlyphService GlyphService { get; set; }

        [Import]
        public IClassificationFormatMapService ClassificationFormatMapService { get; set; }

        [Import]
        public IEditorFormatMapService EditorFormatMapService { get; set; }

        [Import]
        public INQueryClassificationService ClassificationService { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap("text");
            var editorFormatMap = EditorFormatMapService.GetEditorFormatMap("text");
            return new NQueryQuickInfoSource(GlyphService, classificationFormatMap, editorFormatMap, ClassificationService);
        }
    }
}