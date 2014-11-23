using System;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySemanticIssueSquiggleClassifier : CollectionTagger<ISquiggleTag>
    {
        private readonly Workspace _workspace;

        public NQuerySemanticIssueSquiggleClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticIssueSquiggleClassifier).Name, null, document, true)
        {
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
            var codeIssues = semanticModel.GetIssues();

            using (CreateBatch())
            {
                Clear();

                foreach (var codeIssue in codeIssues)
                {
                    var classificationType = codeIssue.Kind == CodeIssueKind.Error
                        ? ClassificationTypes.CompilerError
                        : codeIssue.Kind == CodeIssueKind.Warning
                            ? ClassificationTypes.Warning
                            : null;

                    if (classificationType == null)
                        continue;

                    var snapshotRange = document.Text.ToSnapshotRange(codeIssue.Span);
                    var tag = new SquiggleTag(classificationType, new DirectContentProvider(codeIssue.Description));
                    var tagRange = new TagVersionRange<ISquiggleTag>(snapshotRange, TextRangeTrackingModes.Default, tag);
                    Add(tagRange);
                }
            }
        }
    }
}