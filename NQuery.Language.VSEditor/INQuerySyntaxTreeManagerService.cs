using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySyntaxTreeManagerService
    {
        INQuerySyntaxTreeManager GetSyntaxTreeManager(ITextBuffer textBuffer);
    }
}