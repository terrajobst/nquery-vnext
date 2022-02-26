using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Classifications;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQuerySemanticClassifier : AsyncTagger<IClassificationTag,SemanticClassificationSpan>
    {
        private readonly INQueryClassificationService _classificationService;
        private readonly Workspace _workspace;

        public NQuerySemanticClassifier(INQueryClassificationService classificationService, Workspace workspace)
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

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SemanticClassificationSpan>>> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.SyntaxTree;
            var snapshot = document.GetTextSnapshot();
            var semanticClassificationSpans = await Task.Run(() => syntaxTree.Root.ClassifySemantics(semanticModel));
            return Tuple.Create(snapshot, semanticClassificationSpans.AsEnumerable());
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, SemanticClassificationSpan rawTag)
        {
            var span = rawTag.Span;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            var classification = GetClassificationType(rawTag.Classification);
            var tag = new ClassificationTag(classification);
            return new TagSpan<IClassificationTag>(snapshotSpan, tag);
        }

        private IClassificationType GetClassificationType(SemanticClassification classification)
        {
            switch (classification)
            {
                case SemanticClassification.SchemaTable:
                    return _classificationService.SchemaTable;
                case SemanticClassification.Column:
                    return _classificationService.Column;
                case SemanticClassification.DerivedTable:
                    return _classificationService.DerivedTable;
                case SemanticClassification.CommonTableExpression:
                    return _classificationService.CommonTableExpression;
                case SemanticClassification.Function:
                    return _classificationService.Function;
                case SemanticClassification.Aggregate:
                    return _classificationService.Aggregate;
                case SemanticClassification.Variable:
                    return _classificationService.Variable;
                case SemanticClassification.Property:
                    return _classificationService.Property;
                case SemanticClassification.Method:
                    return _classificationService.Method;
                default:
                    throw ExceptionBuilder.UnexpectedValue(classification);
            }
        }
    }
}