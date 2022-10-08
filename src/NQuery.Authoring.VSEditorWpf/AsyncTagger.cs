using System.Collections.Immutable;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Authoring.VSEditorWpf
{
    public abstract class AsyncTagger<TTag, TRawTag> : ITagger<TTag>
        where TTag : ITag
    {
        private ImmutableArray<TRawTag> _rawTags = ImmutableArray<TRawTag>.Empty;
        private ITextSnapshot _rawTagsSnapshot;

        protected async void InvalidateTagsAsync()
        {
            var (snapshot, rawTags) = await GetRawTagsAsync();
            _rawTagsSnapshot = snapshot;
            _rawTags = rawTags.ToImmutableArray();
            var snapshotSpan = new SnapshotSpan(_rawTagsSnapshot, 0, _rawTagsSnapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        protected abstract Task<(ITextSnapshot Snapshot, IEnumerable<TRawTag> RawTags)> GetRawTagsAsync();
        protected abstract ITagSpan<TTag> CreateTagSpan(ITextSnapshot snapshot, TRawTag rawTag);

        public IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return _rawTags.Select(rawTag => CreateTagSpan(_rawTagsSnapshot, rawTag));
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            handler?.Invoke(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}