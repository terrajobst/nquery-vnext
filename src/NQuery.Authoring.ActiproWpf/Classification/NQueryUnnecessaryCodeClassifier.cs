using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Document;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    internal sealed class NQueryUnnecessaryCodeClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;
        private readonly NQueryDocument _queryDocument;

        public NQueryUnnecessaryCodeClassifier(ICodeDocument document)
            : base(typeof(NQueryUnnecessaryCodeClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();

            _queryDocument = document.GetNQueryDocument();
            if (_queryDocument == null)
                return;

            _queryDocument.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            UpdateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var semanticModel = await _queryDocument.GetSemanticModelAsync();
            if (semanticModel == null)
                return;

            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;

            var unnecessaryCodeSpans = await GetUnnecessaryCodeSpansAsync(semanticModel);
            var tags = from cs in unnecessaryCodeSpans
                       let textRange = textBuffer.ToSnapshotRange(snapshot, cs)
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