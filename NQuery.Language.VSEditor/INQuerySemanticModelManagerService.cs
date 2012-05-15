using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySemanticModelManagerService
    {
        INQuerySemanticModelManager GetSemanticModelManager(ITextBuffer textBuffer);
    }
}