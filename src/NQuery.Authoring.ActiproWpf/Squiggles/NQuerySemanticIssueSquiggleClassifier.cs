using System;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Document;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySemanticIssueSquiggleClassifier : CollectionTagger<ISquiggleTag>
    {
        private readonly NQueryDocument _queryDocument;

        public NQuerySemanticIssueSquiggleClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticIssueSquiggleClassifier).Name, null, document, true)
        {
            _queryDocument = document.GetNQueryDocument();
            if (_queryDocument == null)
                return;

            _queryDocument.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var semanticModel = await _queryDocument.GetSemanticModelAsync();
            var codeIssues = semanticModel.GetIssues();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
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

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }
    }
}