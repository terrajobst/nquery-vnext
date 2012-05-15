using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(INQuerySyntaxTreeManagerService))]
    internal sealed class NQuerySyntaxTreeManagerService : INQuerySyntaxTreeManagerService
    {
        public INQuerySyntaxTreeManager GetSyntaxTreeManager(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new NQuerySyntaxTreeManager(textBuffer));
        }
    }
}