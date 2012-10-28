using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.BraceMatching
{
    internal sealed class NQueryBraceTagger : AsyncTagger<ITextMarkerTag, SnapshotSpan>
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly IBraceMatchingService _braceMatchingService;

        public NQueryBraceTagger(ITextView textView, INQueryDocument document, IBraceMatchingService braceMatchingService)
        {
            _textView = textView;
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _braceMatchingService = braceMatchingService;
            InvalidateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
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
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = _document.GetTextSnapshot(syntaxTree);
            TextSpan left;
            TextSpan right;
            if (!_braceMatchingService.TryFindBrace(syntaxTree, position, out left, out right))
                return Tuple.Create(snapshot, Enumerable.Empty<SnapshotSpan>());

            var leftSpan = new SnapshotSpan(snapshot, left.Start, left.Length);
            var rightSpan = new SnapshotSpan(snapshot, right.Start, right.Length);

            return Tuple.Create(snapshot, new[] { leftSpan, rightSpan }.AsEnumerable());
        }

        protected override ITagSpan<ITextMarkerTag> CreateTagSpan(ITextSnapshot snapshot, SnapshotSpan rawTag)
        {
            var tag = new TextMarkerTag("bracehighlight");
            return new TagSpan<ITextMarkerTag>(rawTag, tag);
        }
    }
}