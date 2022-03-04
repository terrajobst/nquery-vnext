using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQueryUnnecessaryCodeClassifier : AsyncTagger<IClassificationTag, TextSpan>
    {
        private readonly INQueryClassificationService _classificationService;
        private readonly Workspace _workspace;

        public NQueryUnnecessaryCodeClassifier(INQueryClassificationService classificationService, Workspace workspace)
        {
            _classificationService = classificationService;
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            InvalidateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<TextSpan>>> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var snapshot = document.GetTextSnapshot();
            var unnecessarySpans = await Task.Run(() => semanticModel.GetIssues().Where(i => i.Kind == CodeIssueKind.Unnecessary).Select(i => i.Span));
            return Tuple.Create(snapshot, unnecessarySpans);
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, TextSpan rawTag)
        {
            var snapshotSpan = new SnapshotSpan(snapshot, rawTag.Start, rawTag.Length);
            var classification = _classificationService.Unnecessary;
            var tag = new ClassificationTag(classification);
            return new TagSpan<IClassificationTag>(snapshotSpan, tag);
        }
    }
}