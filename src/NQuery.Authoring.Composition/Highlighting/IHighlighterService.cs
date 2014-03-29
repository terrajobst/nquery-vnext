using System;
using System.Collections.Immutable;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.Composition.Highlighting
{
    public interface IHighlighterService
    {
        ImmutableArray<IHighlighter> Highlighters { get; }
    }
}