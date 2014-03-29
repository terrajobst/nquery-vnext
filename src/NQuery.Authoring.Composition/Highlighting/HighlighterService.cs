using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.Composition.Highlighting
{
    [Export(typeof(IHighlighterService))]
    internal sealed class HighlighterService : IHighlighterService
    {
        private readonly ImmutableArray<IHighlighter> _highlighters;

        [ImportingConstructor]
        public HighlighterService([ImportMany] IEnumerable<IHighlighter> highlighters)
        {
            _highlighters = highlighters.Concat(HighlightingExtensions.GetStandardHighlighters()).ToImmutableArray();
        }

        public ImmutableArray<IHighlighter> Highlighters
        {
            get { return _highlighters; }
        }
    }
}