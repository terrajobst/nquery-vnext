using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.VSEditorWpf.Document;
using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    internal sealed class NQueryHighlightingTagger : AsyncTagger<HighlightTag, SnapshotSpan>
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly IReadOnlyCollection<IHighlighter> _highlighters;

        public NQueryHighlightingTagger(ITextView textView, INQueryDocument document, IReadOnlyCollection<IHighlighter> highlighters)
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
            var position = _textView.Caret.Position.BufferPosition.Position;
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var snapshot = _document.GetTextSnapshot(syntaxTree);

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