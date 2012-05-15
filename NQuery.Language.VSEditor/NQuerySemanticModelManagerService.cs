using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(INQuerySemanticModelManagerService))]
    internal sealed class NQuerySemanticModelManagerService : INQuerySemanticModelManagerService
    {
        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        public INQuerySemanticModelManager GetSemanticModelManager(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                var syntaxTreeManager = SyntaxTreeManagerService.GetSyntaxTreeManager(textBuffer);
                return new NQuerySemanticModelManager(syntaxTreeManager);
            });
        }
    }
}