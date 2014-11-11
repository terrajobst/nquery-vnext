using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SnapshotSpan>>> GetRawTagsAsync()
        {
            var documentView = _textView.GetDocumentView();
            var position = documentView.Position;
            var document = documentView.Document;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var snapshot = document.GetTextSnapshot();
            var result = syntaxTree.MatchBraces(position, _braceMatcherService.Matchers);
            if (!result.IsValid)
                return Tuple.Create(snapshot, Enumerable.Empty<SnapshotSpan>());

            var leftSpan = new SnapshotSpan(snapshot, result.Left.Start, result.Left.Length);
            var rightSpan = new SnapshotSpan(snapshot, result.Right.Start, result.Right.Length);

            return Tuple.Create(snapshot, new[] { leftSpan, rightSpan }.AsEnumerable());
        }

        protected override ITagSpan<ITextMarkerTag> CreateTagSpan(ITextSnapshot snapshot, SnapshotSpan rawTag)
        {
            var tag = new TextMarkerTag("bracehighlight");
            return new TagSpan<ITextMarkerTag>(rawTag, tag);
        }
    }
}