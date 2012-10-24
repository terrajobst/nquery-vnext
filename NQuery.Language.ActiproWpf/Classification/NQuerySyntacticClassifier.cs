using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Language.Services.Classifications;

namespace NQuery.Language.ActiproWpf.Classification
{
    internal sealed class NQuerySyntacticClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;

        public NQuerySyntacticClassifier(ICodeDocument document)
            : base(typeof(NQuerySyntacticClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();
            document.ParseDataChanged += DocumentOnParseDataChanged;
            UpdateTags();
        }

        private void DocumentOnParseDataChanged(object sender, ParseDataPropertyChangedEventArgs e)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var parseData = await Document.GetParseDataAsync();
            var snapshot = parseData.Snapshot;
            var textBuffer = parseData.SyntaxTree.TextBuffer;
            var classificationSpans = await ClassifyTreeAsync(parseData.SyntaxTree);

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

        private IClassificationType GetClassification(SyntaxClassification kind)
        {
            switch (kind)
            {
                case SyntaxClassification.Whitespace:
                    return _classificationTypes.WhiteSpace;
                case SyntaxClassification.Comment:
                    return _classificationTypes.Comment;
                case SyntaxClassification.Keyword:
                    return _classificationTypes.Keyword;
                case SyntaxClassification.Punctuation:
                    return _classificationTypes.Punctuation;
                case SyntaxClassification.Identifier:
                    return _classificationTypes.Identifier;
                case SyntaxClassification.StringLiteral:
                    return _classificationTypes.StringLiteral;
                case SyntaxClassification.NumberLiteral:
                    return _classificationTypes.NumberLiteral;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }

        private static Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifyTreeAsync(SyntaxTree syntaxTree)
        {
            return Task.Run(() => syntaxTree.Root.ClassifySyntax());
        }
    }
}