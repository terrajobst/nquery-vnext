using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Language.VSEditor
{
    internal abstract class NQueryErrorTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly string _errorType;

        protected NQueryErrorTagger(ITextBuffer textBuffer, string errorType)
        {
            _textBuffer = textBuffer;
            _errorType = errorType;
        }

        protected void InvalidateTags()
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        protected abstract IEnumerable<Diagnostic> GetDiagnostics();

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<ITagSpan<IErrorTag>>();
            var diagnostics = GetDiagnostics();
            var snapshot = _textBuffer.CurrentSnapshot;

            foreach (var diagnostic in diagnostics)
            {
                var span = new Span(diagnostic.Span.Start, diagnostic.Span.Length);
                var snapshotSpan = new SnapshotSpan(snapshot, span);
                var errorTag = new ErrorTag(_errorType, diagnostic.Message);
                var errorTagSpan = new TagSpan<IErrorTag>(snapshotSpan, errorTag);
                result.Add(errorTagSpan);
            }

            return result;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}