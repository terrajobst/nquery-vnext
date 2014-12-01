using System;
using System.Collections.Immutable;

using NQuery.Authoring.Rearrangement;

namespace NQuery.Authoring.Composition.Rearrangement
{
    public interface IRearrangersService
    {
        ImmutableArray<IRearranger> Rearrangers { get; }
    }
}