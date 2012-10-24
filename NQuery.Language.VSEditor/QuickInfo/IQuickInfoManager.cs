using System;

using NQuery.Language.Services.QuickInfo;

namespace NQuery.Language.VSEditor.QuickInfo
{
    public interface IQuickInfoManager
    {
        void TriggerQuickInfo(int offset);

        QuickInfoModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}