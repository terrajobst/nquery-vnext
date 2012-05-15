using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("NQueryQuickInfoSourceProvider")]
    [ContentType("NQuery")]
    internal sealed class NQueryQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(textBuffer);
            return new NQueryQuickInfoSource(semanticModelManager);
        }
    }
}