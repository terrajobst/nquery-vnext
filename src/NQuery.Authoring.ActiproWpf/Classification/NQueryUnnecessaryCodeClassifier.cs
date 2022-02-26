using System.Collections.Immutable;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    internal sealed class NQueryUnnecessaryCodeClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;
        private readonly Workspace _workspace;

        public NQueryUnnecessaryCodeClassifier(ICodeDocument document)
            : base(typeof(NQueryUnnecessaryCodeClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();

            _workspace = document.GetWorkspace();
            if (_workspace == null)
                return;

            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var unnecessaryCodeSpans = await GetUnnecessaryCodeSpansAsync(semanticModel);

            var tags = from cs in unnecessaryCodeSpans
                       let textRange = document.Text.ToSnapshotRange(cs)
                       let classificationType = _classificationTypes.Unnecessary
                       let tag = new ClassificationTag(classificationType)
                       select new TagVersionRange<IClassificationTag>(textRange, TextRangeTrackingModes.Default, tag);

            using (CreateBatch())
            {
                Clear();
                foreach (var tag in tags)
                    Add(tag);
            }
        }

        private static Task<IEnumerable<TextSpan>> GetUnnecessaryCodeSpansAsync(SemanticModel semanticModel)
        {
            return Task.Run<IEnumerable<TextSpan>>(() => semanticModel.GetIssues()
                                                                      .Where(i => i.Kind == CodeIssueKind.Unnecessary)
                                                                      .Select(i => i.Span)
                                                                      .ToImmutableArray());
        }
    }
}