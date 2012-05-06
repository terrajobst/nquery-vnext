using System;

using Microsoft.VisualStudio.Text;

namespace NQueryViewer.EditorIntegration
{
    public interface INQuerySyntaxTreeManagerService
    {
        INQuerySyntaxTreeManager GetCSharpSyntaxTreeManager(ITextBuffer textBuffer);
    }
}