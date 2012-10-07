using System;

namespace NQuery.Language.VSEditor
{
    public interface IQuickInfoManager
    {
        void TriggerQuickInfo(int offset);

        QuickInfoModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}