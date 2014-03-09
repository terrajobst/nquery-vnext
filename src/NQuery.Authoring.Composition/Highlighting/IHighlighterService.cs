using System;
using System.Collections.Generic;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.Composition.Highlighting
{
    public interface IHighlighterService
    {
        IReadOnlyCollection<IHighlighter> Highlighters { get; }
    }
}