using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Classifications;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQuerySyntaxClassifier : AsyncTagger<IClassificationTag, SyntaxClassificationSpan>
    {
        private readonly INQueryClassificationService _classificationService;
        private readonly Workspace _workspace;

        public NQuerySyntaxClassifier(INQueryClassificationService classificationService, Workspace workspace)
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

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SyntaxClassificationSpan>>> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var snapshot = document.GetTextSnapshot();
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
                    return _classificationService.Punctuation;
                case SyntaxClassification.Identifier:
                    return _classificationService.Identifier;
                case SyntaxClassification.StringLiteral:
                    return _classificationService.StringLiteral;
                case SyntaxClassification.NumberLiteral:
                    return _classificationService.NumberLiteral;
                default:
                    throw ExceptionBuilder.UnexpectedValue(classification);
            }
        }
    }
}