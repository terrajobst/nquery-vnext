using System.Collections.Immutable;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    internal sealed class NQueryHighlightingTagger : AsyncTagger<HighlightTag, SnapshotSpan>
    {
        private readonly Workspace _workspace;
        private readonly ITextView _textView;
        private readonly ImmutableArray<IHighlighter> _highlighters;

        public NQueryHighlightingTagger(Workspace workspace, ITextView textView, ImmutableArray<IHighlighter> highlighters)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textView = textView;
            _highlighters = highlighters;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            InvalidateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<(ITextSnapshot Snapshot, IEnumerable<SnapshotSpan> RawTags)> GetRawTagsAsync()
        {
            var documentView = _textView.GetDocumentView();
            var position = documentView.Position;
            var document = documentView.Document;
            var snapshot = document.GetTextSnapshot();
            var semanticModel = await document.GetSemanticModelAsync();

            var spans = semanticModel.GetHighlights(position, _highlighters)
                                     .Select(span => new SnapshotSpan(snapshot, span.Start, span.Length));

            return (snapshot, spans);
        }

        protected override ITagSpan<HighlightTag> CreateTagSpan(ITextSnapshot snapshot, SnapshotSpan rawTag)
        {
            var tag = new HighlightTag();
            return new TagSpan<HighlightTag>(rawTag, tag);
        }
    }
}