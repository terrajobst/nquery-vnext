using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Document;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Outlining
{
    internal sealed class NQueryOutliningTagger : AsyncTagger<IOutliningRegionTag, OutliningRegionSpan>
    {
        private readonly NQueryDocument _document;

        public NQueryOutliningTagger(NQueryDocument document)
        {
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<OutliningRegionSpan>>> GetRawTagsAsync()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = syntaxTree.GetTextSnapshot();
            var result = syntaxTree.Root.FindRegions();
            return Tuple.Create(snapshot, result.AsEnumerable());
        }

        protected override ITagSpan<IOutliningRegionTag> CreateTagSpan(ITextSnapshot snapshot, OutliningRegionSpan rawTag)
        {
            var textSpan = rawTag.Span;
            var span = new Span(textSpan.Start, textSpan.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var hint = snapshot.GetText(span);
            var tag = new OutliningRegionTag(false, false, rawTag.Text, hint);
            var tagSpan = new TagSpan<IOutliningRegionTag>(snapshotSpan, tag);
            return tagSpan;
        }
    }
}