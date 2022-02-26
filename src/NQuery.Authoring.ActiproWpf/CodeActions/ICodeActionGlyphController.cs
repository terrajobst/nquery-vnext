namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    public interface ICodeActionGlyphController
    {
        bool IsActive { get; }
        bool IsExpanded { get; }
        void Expand();
        void Collapse();
    }
}