using System;

namespace NQuery.Language.VSEditor.Selection
{
    public interface INQuerySelectionProvider
    {
        void ExtendSelection();
        void ShrinkSelection();
    }
}