namespace NQuery.Language.VSEditor
{
    public interface INQuerySelectionProvider
    {
        void ExtendSelection();
        void ShrinkSelection();
    }
}