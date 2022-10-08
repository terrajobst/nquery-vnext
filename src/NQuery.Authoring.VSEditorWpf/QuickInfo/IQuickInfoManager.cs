using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    public interface IQuickInfoManager
    {
        Task TriggerQuickInfoAsync(int offset);

        QuickInfoModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}