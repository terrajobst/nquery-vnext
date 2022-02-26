using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Authoring.Classifications;

namespace NQuery.Authoring.ActiproWpf.Classification
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
            var document = Document.GetDocument();
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var classificationSpans = await ClassifyTreeAsync(syntaxTree);

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
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifyTreeAsync(SyntaxTree syntaxTree)
        {
            return Task.Run(() => syntaxTree.Root.ClassifySyntax());
        }
    }
}