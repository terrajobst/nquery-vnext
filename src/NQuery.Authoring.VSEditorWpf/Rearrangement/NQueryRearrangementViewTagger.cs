using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.VSEditorWpf.Classification;
using NQuery.Authoring.VSEditorWpf.Text;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    internal sealed class NQueryRearrangementViewTagger : ITagger<IClassificationTag>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly IRearrangeModelManager _rearrangeModelManager;
        private readonly INQueryClassificationService _classificationService;

        public NQueryRearrangementViewTagger(ITextBuffer textBuffer, IRearrangeModelManager rearrangeModelManager, INQueryClassificationService classificationService)
        {
            _textBuffer = textBuffer;
            _rearrangeModelManager = rearrangeModelManager;
            _classificationService = classificationService;
            _rearrangeModelManager.IsVisibleChanged += RearrangeModelManagerOnIsVisibleChanged;
            _rearrangeModelManager.ModelChanged += RearrangeModelManagerOnModelChanged;
            InvalidateTags();
        }

        private void RearrangeModelManagerOnIsVisibleChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        private void RearrangeModelManagerOnModelChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var model = _rearrangeModelManager.Model;

            if (model == null || !_rearrangeModelManager.IsVisible)
                yield break;

            var arrangement = model.Arrangement;
            var snapshot = model.SyntaxTree.Text.ToTextSnapshot();
            var verticalOperation = arrangement.VerticalOperation;
            var horizontalOperation = arrangement.HorizontalOperation;

            if (verticalOperation != null)
                yield return CreateTag(snapshot, verticalOperation.Span, _classificationService.RearrangeVertically);

            if (horizontalOperation != null)
                yield return CreateTag(snapshot, horizontalOperation.Span, _classificationService.RearrangeHorizontally);
        }

        private static ITagSpan<IClassificationTag> CreateTag(ITextSnapshot snapshot, TextSpan textSpan, IClassificationType classificationType)
        {
            var span = new SnapshotSpan(snapshot, textSpan.Start, textSpan.Length);
            var tag = new ClassificationTag(classificationType);
            var tagSpan = new TagSpan<IClassificationTag>(span, tag);
            return tagSpan;
        }

        private void InvalidateTags()
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var length = snapshot.Length;
            var span = new SnapshotSpan(snapshot, 0, length);
            var args = new SnapshotSpanEventArgs(span);
            OnTagsChanged(args);
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