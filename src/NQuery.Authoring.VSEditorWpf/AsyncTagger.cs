using System.Collections.Immutable;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Authoring.VSEditorWpf
{
    public abstract class AsyncTagger<TTag, TRawTag> : ITagger<TTag>
        where TTag: ITag
    {
        private ImmutableArray<TRawTag> _rawTags = ImmutableArray<TRawTag>.Empty;
        private ITextSnapshot _rawTagsSnapshot;

        protected async void InvalidateTags()
        {
            var rawTagsResult = await GetRawTagsAsync();
            _rawTagsSnapshot = rawTagsResult.Item1;
            _rawTags = rawTagsResult.Item2.ToImmutableArray();
            var snapshotSpan = new SnapshotSpan(_rawTagsSnapshot, 0, _rawTagsSnapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        protected abstract Task<Tuple<ITextSnapshot, IEnumerable<TRawTag>>> GetRawTagsAsync();
        protected abstract ITagSpan<TTag> CreateTagSpan(ITextSnapshot snapshot, TRawTag rawTag);

        public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return _rawTags.Select(rawTag => CreateTagSpan(_rawTagsSnapshot, rawTag));
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