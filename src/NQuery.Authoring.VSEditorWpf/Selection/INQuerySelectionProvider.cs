namespace NQuery.Authoring.VSEditorWpf.Selection
{
    public interface INQuerySelectionProvider
    {
        Task ExtendSelectionAsync();
        void ShrinkSelection();
    }
}