using System;
using System.Collections.Immutable;

using NQuery.Authoring.Outlining;

namespace NQuery.Authoring.Composition.Outlining
{
    public interface IOutliningService
    {
        ImmutableArray<IOutliner> Outliners { get; }
    }
}