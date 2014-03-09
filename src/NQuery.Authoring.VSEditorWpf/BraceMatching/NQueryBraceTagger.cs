using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.Composition.BraceMatching;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.BraceMatching
{
    internal sealed class NQueryBraceTagger : AsyncTagger<ITextMarkerTag, SnapshotSpan>
    {
        private readonly ITextView _textView;
        private readonly INQueryDocument _document;
        private readonly IBraceMatcherService _braceMatcherService;

        public NQueryBraceTagger(ITextView textView, INQueryDocument document, IBraceMatcherService braceMatcherService)
        {
            _textView = textView;
            _document = document;
            _braceMatcherService = braceMatcherService;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
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
            var result = syntaxTree.FindBrace(position, _braceMatcherService.Matchers);
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