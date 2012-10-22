using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    internal abstract class NQuerySquiggleClassifier : CollectionTagger<ISquiggleTag>
    {
        private readonly IClassificationType _classificationType;

        protected NQuerySquiggleClassifier(IClassificationType classificationType, string key, IEnumerable<Ordering> orderings, ICodeDocument document, bool isForLanguage)
            : base(key, orderings, document, isForLanguage)
        {
            _classificationType = classificationType;
        }

        protected async void UpdateTags()
        {
            var versionAndDiagnostics = await GetDiagnosticsAync();
            var parseData = versionAndDiagnostics.Item1;
            var snapshot = parseData.Snapshot;
            var textBuffer = parseData.SyntaxTree.TextBuffer;
            var diagnostics = versionAndDiagnostics.Item2;

            using (CreateBatch())
            {
                Clear();
                
                foreach (var diagnostic in diagnostics)
                {
                    var snapshotRange = textBuffer.ToSnapshotRange(snapshot, diagnostic.Span);
                    var tag = new SquiggleTag(_classificationType, new DirectContentProvider(diagnostic.Message));
                    var tagRange = new TagVersionRange<ISquiggleTag>(snapshotRange, TextRangeTrackingModes.Default, tag);
                    Add(tagRange);
                }
            }
        }

        protected abstract Task<Tuple<NQueryParseData, IEnumerable<Diagnostic>>> GetDiagnosticsAync();
    }
}