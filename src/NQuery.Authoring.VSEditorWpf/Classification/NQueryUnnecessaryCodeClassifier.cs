using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Document;
using NQuery.Authoring.VSEditorWpf.Document;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQueryUnnecessaryCodeClassifier : AsyncTagger<IClassificationTag,TextSpan>
    {
        private readonly INQueryClassificationService _classificationService;
        private readonly NQueryDocument _document;

        public NQueryUnnecessaryCodeClassifier(INQueryClassificationService classificationService, NQueryDocument document)
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

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<TextSpan>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var snapshot = semanticModel.GetTextSnapshot();
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