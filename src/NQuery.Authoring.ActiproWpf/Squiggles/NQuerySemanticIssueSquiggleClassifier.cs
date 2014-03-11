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
        public NQuerySemanticIssueSquiggleClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            var nqueryDocument = document as NQueryDocument;
            if (nqueryDocument != null)
            {
                nqueryDocument.SemanticDataChanged += NqueryDocumentOnSemanticDataChanged;
                UpdateTags();
            }
        }

        private async void UpdateTags()
        {
            var semanticData = await Document.GetSemanticDataAsync();
            var codeIssues = semanticData.SemanticModel.GetIssues();
            var syntaxTree = semanticData.SemanticModel.Compilation.SyntaxTree;
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;

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

                    var snapshotRange = textBuffer.ToSnapshotRange(snapshot, codeIssue.Span);
                    var tag = new SquiggleTag(classificationType, new DirectContentProvider(codeIssue.Description));
                    var tagRange = new TagVersionRange<ISquiggleTag>(snapshotRange, TextRangeTrackingModes.Default, tag);
                    Add(tagRange);
                }
            }
        }

        private void NqueryDocumentOnSemanticDataChanged(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }
    }
}