using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.Composition.BraceMatching;

namespace NQuery.Authoring.VSEditorWpf.BraceMatching
{
    internal sealed class NQueryBraceTagger : AsyncTagger<ITextMarkerTag, SnapshotSpan>
    {
        private readonly Workspace _workspace;
        private readonly ITextView _textView;
        private readonly IBraceMatcherService _braceMatcherService;

        public NQueryBraceTagger(Workspace workspace, ITextView textView, IBraceMatcherService braceMatcherService)
        {
            _workspace = workspace;
            _textView = textView;
            _braceMatcherService = braceMatcherService;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            InvalidateTagsAsync();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTagsAsync();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTagsAsync();
        }

        protected override async Task<(ITextSnapshot Snapshot, IEnumerable<SnapshotSpan> RawTags)> GetRawTagsAsync()
        {
            var documentView = _textView.GetDocumentView();
            var position = documentView.Position;
            var document = documentView.Document;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var snapshot = document.GetTextSnapshot();
            var result = syntaxTree.MatchBraces(position, _braceMatcherService.Matchers);
            if (!result.IsValid)
                return (snapshot, Enumerable.Empty<SnapshotSpan>());

            var leftSpan = new SnapshotSpan(snapshot, result.Left.Start, result.Left.Length);
            var rightSpan = new SnapshotSpan(snapshot, result.Right.Start, result.Right.Length);

            return (snapshot, new[] { leftSpan, rightSpan }.AsEnumerable());
        }

        protected override ITagSpan<ITextMarkerTag> CreateTagSpan(ITextSnapshot snapshot, SnapshotSpan rawTag)
        {
            var tag = new TextMarkerTag(@"bracehighlight");
            return new TagSpan<ITextMarkerTag>(rawTag, tag);
        }
    }
}