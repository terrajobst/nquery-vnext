using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.ActiproWpf.Squiggles;

internal abstract class NQuerySquiggleClassifier : CollectionTagger<ISquiggleTag>
{
    private readonly IClassificationType _classificationType;

    protected NQuerySquiggleClassifier(IClassificationType classificationType, string key, IEnumerable<Ordering>? orderings, ICodeDocument document, bool isForLanguage)
        : base(key, orderings, document, isForLanguage)
    {
        ThrowIfNull(classificationType);
        ThrowIfNull(key);
        ThrowIfNull(document);

        _classificationType = classificationType;
    }

    protected async void UpdateTagsAsync()
    {
        var versionAndDiagnostics = await GetDiagnosticsAsync();
        var text = versionAndDiagnostics.Text;
        var diagnostics = versionAndDiagnostics.Diagnostics;

        using (CreateBatch())
        {
            Clear();

            foreach (var diagnostic in diagnostics)
            {
                var snapshotRange = text.ToSnapshotRange(diagnostic.Span);
                var tag = new SquiggleTag(_classificationType, new DirectContentProvider(diagnostic.Message));
                var tagRange = new TagVersionRange<ISquiggleTag>(snapshotRange, TextRangeTrackingModes.Default, tag);
                Add(tagRange);
            }
        }
    }

    protected abstract Task<(SourceText Text, IEnumerable<Diagnostic> Diagnostics)> GetDiagnosticsAsync();
}
