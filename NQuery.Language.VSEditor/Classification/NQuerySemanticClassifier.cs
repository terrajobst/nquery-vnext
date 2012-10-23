using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticClassifier : AsyncTagger<IClassificationTag,SemanticClassificationSpan>
    {
        private readonly INQuerySemanticClassificationService _classificationService;
        private readonly INQueryDocument _document;

        public NQuerySemanticClassifier(INQuerySemanticClassificationService classificationService, INQueryDocument document)
        {
            _classificationService = classificationService;
            _document = document;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SemanticClassificationSpan>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var snapshot = _document.GetTextSnapshot(syntaxTree);
            var semanticClassificationSpans = await Task.Run(() => syntaxTree.Root.ClassifySemantic(semanticModel));
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
                    throw new ArgumentException("classification");
            }
        }
    }
}