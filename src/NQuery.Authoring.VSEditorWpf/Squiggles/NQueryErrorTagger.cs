using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal abstract class NQueryErrorTagger : AsyncTagger<IErrorTag, Diagnostic>
    {
        private readonly string _errorType;

        protected NQueryErrorTagger(string errorType)
        {
            _errorType = errorType;
        }

        protected override ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, Diagnostic rawTag)
        {
            var span = new Span(rawTag.Span.Start, rawTag.Span.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var errorTag = new ErrorTag(_errorType, rawTag.Message);
            var errorTagSpan = new TagSpan<IErrorTag>(snapshotSpan, errorTag);
            return errorTagSpan;
        }
    }
}