using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Authoring.Classifications;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    internal sealed class NQuerySemanticClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;
        private readonly Workspace _workspace;

        public NQuerySemanticClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticClassifier).Name, null, document, true)
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
            var classificationSpans = await ClassifyAsync(semanticModel);

            var tags = from cs in classificationSpans
                       let textRange = document.Text.ToSnapshotRange(cs.Span)
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
                    throw new ArgumentOutOfRangeException(nameof(classification));
            }
        }

        private static Task<IReadOnlyList<SemanticClassificationSpan>> ClassifyAsync(SemanticModel semanticModel)
        {
            return Task.Run(() =>
            {
                var root = semanticModel.SyntaxTree.Root;
                return root.ClassifySemantics(semanticModel);
            });
        }
    }
}