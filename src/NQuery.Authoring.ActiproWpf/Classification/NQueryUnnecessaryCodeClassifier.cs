using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    internal sealed class NQueryUnnecessaryCodeClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;

        public NQueryUnnecessaryCodeClassifier(ICodeDocument document)
            : base(typeof(NQueryUnnecessaryCodeClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();

            var queryDocument = document as NQueryDocument;
            if (queryDocument == null)
                return;

            queryDocument.SemanticDataChanged += DocumentOnSemanticDataChanged;
            UpdateTags();
        }

        private void DocumentOnSemanticDataChanged(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var semanticData = await Document.GetSemanticDataAsync();
            if (semanticData == null)
                return;

            var syntaxTree = semanticData.SemanticModel.Compilation.SyntaxTree;
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;

            var codeIssues = await GetCodeIssuesAsync(semanticData);
            var tags = from cs in codeIssues
                let textRange = textBuffer.ToSnapshotRange(snapshot, cs.Span)
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

        private Task<IEnumerable<CodeIssue>> GetCodeIssuesAsync(NQuerySemanticData semanticData)
        {
            return Task.Run<IEnumerable<CodeIssue>>(() => semanticData.SemanticModel.GetIssues().ToArray());
        }
    }
}