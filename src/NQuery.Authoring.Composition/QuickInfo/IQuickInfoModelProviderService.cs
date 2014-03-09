using System;
using System.Collections.Generic;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.Composition.QuickInfo
{
    public interface IQuickInfoModelProviderService
    {
        IReadOnlyCollection<IQuickInfoModelProvider> Providers { get; }
    }
}