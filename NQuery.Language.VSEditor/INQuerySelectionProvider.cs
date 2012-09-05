namespace NQuery.Language.VSEditor
{
    public interface INQuerySelectionProvider
    {
        bool ExtendSelection();
        bool ShrinkSelection();
    }
}