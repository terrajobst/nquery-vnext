using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

namespace NQueryViewer.EditorIntegration
{
    [Export(typeof(INQuerySyntaxTreeManagerService))]
    internal class InQuerySyntaxTreeManagerService : INQuerySyntaxTreeManagerService
    {
        public INQuerySyntaxTreeManager GetCSharpSyntaxTreeManager(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new NQuerySyntaxTreeManager(textBuffer));
        }
    }

    public interface INQuerySyntaxTreeManagerService
    {
        INQuerySyntaxTreeManager GetCSharpSyntaxTreeManager(ITextBuffer textBuffer);
    }
}