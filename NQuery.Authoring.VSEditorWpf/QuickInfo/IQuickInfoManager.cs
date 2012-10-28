using System;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    public interface IQuickInfoManager
    {
        void TriggerQuickInfo(int offset);

        QuickInfoModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}