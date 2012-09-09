using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Language.VSEditor.BraceMatching
{
    internal sealed class NQueryBraceTagger : ITagger<ITextMarkerTag>
    {
        private readonly ITextView _textView;
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;
        private readonly IBraceMatchingService _braceMatchingService;

        public NQueryBraceTagger(ITextView textView, ITextBuffer textBuffer, INQuerySyntaxTreeManager syntaxTreeManager, IBraceMatchingService braceMatchingService)
        {
            _textView = textView;
            _textBuffer = textBuffer;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _syntaxTreeManager = syntaxTreeManager;
            _braceMatchingService = braceMatchingService;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTags();
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        private void InvalidateTags()
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        public IEnumerable<ITagSpan<ITextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var position = _textView.Caret.Position.BufferPosition.Position;
            TextSpan left;
            TextSpan right;
            if (!FindBraces(position, out left, out right))
                return Enumerable.Empty<ITagSpan<ITextMarkerTag>>();

            return new[]
            {
                CreateBraceTag(left),
                CreateBraceTag(right)
            };
        }

        private bool FindBraces(int position, out TextSpan left, out TextSpan right)
        {
            left = default(TextSpan);
            right = default(TextSpan);

            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            return syntaxTree != null && _braceMatchingService.TryFindBrace(syntaxTree, position, out left, out right);
        }

        private ITagSpan<ITextMarkerTag> CreateBraceTag(TextSpan textSpan)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, textSpan.Start, textSpan.Length);
            var tag = new TextMarkerTag("bracehighlight");
            var tagSpan = new TagSpan<ITextMarkerTag>(snapshotSpan, tag);
            return tagSpan;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}