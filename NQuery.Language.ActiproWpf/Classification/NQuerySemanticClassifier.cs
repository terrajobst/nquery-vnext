using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Language;
using NQuery.Language.VSEditor;

namespace NQueryViewerActiproWpf
{
    internal sealed class NQuerySemanticClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;

        public NQuerySemanticClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();

            var queryDocument = document as NQueryDocument;
            if (queryDocument == null)
                return;

            queryDocument.SemanticModelChanged += DocumentOnSemanticModelChanged;
            UpdateTags();
        }

        private void DocumentOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var semanticData = await Document.GetSemanticDataAsync();
            if (semanticData == null)
                return;

            var parseData = semanticData.ParseData;
            var snapshot = parseData.Snapshot;
            var textBuffer = parseData.SyntaxTree.TextBuffer;
            var classificationSpans = await ClassifyAsync(semanticData.SemanticModel);

            var tags = from cs in classificationSpans
                       let textRange = textBuffer.ToSnapshotRange(snapshot, cs.Span)
                       let classificationType = GetClassification(cs.Classification)
                       let tag = new ClassificationTag(classificationType)
                       select new TagVersionRange<IClassificationTag>(textRange, TextRangeTrackingModes.Default, tag);

            using (CreateBatch())
            {
                Clear();
                foreach (var tag in tags)
                    Add(tag);
            }
        }

        private IClassificationType GetClassification(SemanticClassification classification)
        {
            switch (classification)
            {
                case SemanticClassification.SchemaTable:
                    return _classificationTypes.SchemaTable;
                case SemanticClassification.Column:
                    return _classificationTypes.Column;
                case SemanticClassification.DerivedTable:
                    return _classificationTypes.DerivedTable;
                case SemanticClassification.CommonTableExpression:
                    return _classificationTypes.CommonTableExpression;
                case SemanticClassification.Function:
                    return _classificationTypes.Function;
                case SemanticClassification.Aggregate:
                    return _classificationTypes.Aggregate;
                case SemanticClassification.Variable:
                    return _classificationTypes.Variable;
                case SemanticClassification.Property:
                    return _classificationTypes.Property;
                case SemanticClassification.Method:
                    return _classificationTypes.Method;
                default:
                    throw new ArgumentOutOfRangeException("classification");
            }
        }

        private static Task<IReadOnlyList<SemanticClassificationSpan>> ClassifyAsync(SemanticModel semanticModel)
        {
            return Task.Run(() =>
            {
                var root = semanticModel.Compilation.SyntaxTree.Root;
                return root.ClassifySemantic(semanticModel);
            });
        }
    }
}