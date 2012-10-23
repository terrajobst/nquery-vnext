using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySyntaxClassifier : AsyncTagger<IClassificationTag, SyntaxClassificationSpan>
    {
        private readonly IStandardClassificationService _classificationService;
        private readonly INQueryDocument _document;

        public NQuerySyntaxClassifier(IStandardClassificationService classificationService, INQueryDocument document)
        {
            _classificationService = classificationService;
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SyntaxClassificationSpan>>> GetRawTagsAsync()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = _document.GetTextSnapshot(syntaxTree);
            var classificationSpans = await Task.Run(() => syntaxTree.Root.ClassifySyntax());
            return Tuple.Create(snapshot, classificationSpans.AsEnumerable());
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, SyntaxClassificationSpan rawTag)
        {
            var span = rawTag.Span;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            var type = GetClassificationType(rawTag.Classification);
            var tag = new ClassificationTag(type);
            return new TagSpan<IClassificationTag>(snapshotSpan, tag);
        }

        private IClassificationType GetClassificationType(SyntaxClassification classification)
        {
            switch (classification)
            {
                case SyntaxClassification.Whitespace:
                    return _classificationService.WhiteSpace;
                case SyntaxClassification.Comment:
                    return _classificationService.Comment;
                case SyntaxClassification.Keyword:
                    return _classificationService.Keyword;
                 case SyntaxClassification.Punctuation:
                    return _classificationService.Operator;
                case SyntaxClassification.Identifier:
                    return _classificationService.Identifier;
                case SyntaxClassification.StringLiteral:
                    return _classificationService.StringLiteral;
                case SyntaxClassification.NumberLiteral:
                    return _classificationService.NumberLiteral;
                default:
                    throw new ArgumentOutOfRangeException("classification");
            }
        }
    }
}