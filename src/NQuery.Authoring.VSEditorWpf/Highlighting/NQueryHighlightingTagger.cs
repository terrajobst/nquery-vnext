using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Document;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    internal sealed class NQueryHighlightingTagger : AsyncTagger<HighlightTag, SnapshotSpan>
    {
        private readonly ITextView _textView;
        private readonly NQueryDocument _document;
        private readonly ImmutableArray<IHighlighter> _highlighters;

        public NQueryHighlightingTagger(ITextView textView, NQueryDocument document, ImmutableArray<IHighlighter> highlighters)
        {
            _textView = textView;
            _document = document;
            _highlighters = highlighters;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            InvalidateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SnapshotSpan>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var snapshot = semanticModel.GetTextSnapshot();
            var position = _textView.GetCaretPosition(snapshot);

            var spans = semanticModel.GetHighlights(position, _highlighters)
                                     .Select(span => new SnapshotSpan(snapshot, span.Start, span.Length));

            return Tuple.Create(snapshot, spans);
        }

        protected override ITagSpan<HighlightTag> CreateTagSpan(ITextSnapshot snapshot, SnapshotSpan rawTag)
        {
            var tag = new HighlightTag();
            return new TagSpan<HighlightTag>(rawTag, tag);
        }
    }
}