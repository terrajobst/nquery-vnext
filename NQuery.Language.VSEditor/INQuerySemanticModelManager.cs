using System;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySemanticModelManager
    {
        Compilation Compilation { get; set; }
        SemanticModel SemanticModel { get; }

        event EventHandler<EventArgs> SemanticModelChanged;
    }
}