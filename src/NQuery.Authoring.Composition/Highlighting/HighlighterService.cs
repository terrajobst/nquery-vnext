using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.Composition.Highlighting
{
    [Export(typeof(IHighlighterService))]
    internal sealed class HighlighterService : IHighlighterService
    {
        [ImportingConstructor]
        public HighlighterService([ImportMany] IEnumerable<IHighlighter> highlighters)
        {
            Highlighters = highlighters.Concat(HighlightingExtensions.GetStandardHighlighters()).ToImmutableArray();
        }

        public ImmutableArray<IHighlighter> Highlighters { get; }
    }
}