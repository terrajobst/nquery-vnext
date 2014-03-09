using System;
using System.Collections.ObjectModel;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.ActiproWpf.QuickInfo
{
    public interface INQueryQuickInfoProvider : IQuickInfoProvider
    {
        Collection<IQuickInfoModelProvider> Providers { get; }
    }
}