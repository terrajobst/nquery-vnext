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
    internal sealed class NQuerySyntaxClassifier : AsyncTagger<IClassificationTag, RawSyntaxTag>
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

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<RawSyntaxTag>>> GetRawTagsAsync()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = _document.GetTextSnapshot(syntaxTree);

            var tags = new List<RawSyntaxTag>();
            var worker = new ClassificationWorker(tags);
            worker.ClassifyNode(syntaxTree.Root);
            return Tuple.Create(snapshot, tags.AsEnumerable());
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, RawSyntaxTag rawTag)
        {
            var span = rawTag.TextSpan;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            var type = GetClassificationForToken(rawTag.Kind);
            var tag = new ClassificationTag(type);
            var tagSpan = new TagSpan<IClassificationTag>(snapshotSpan, tag);
            return tagSpan;
        }

        private IClassificationType GetClassificationForToken(RawSyntaxTagKind kind)
        {
            switch (kind)
            {
                case RawSyntaxTagKind.Keyword:
                    return _classificationService.Keyword;
                case RawSyntaxTagKind.Operator:
                    return _classificationService.Operator;
                case RawSyntaxTagKind.Identifier:
                    return _classificationService.Identifier;
                case RawSyntaxTagKind.StringLiteral:
                    return _classificationService.StringLiteral;
                case RawSyntaxTagKind.NumberLiteral:
                    return _classificationService.NumberLiteral;
                case RawSyntaxTagKind.Comment:
                    return _classificationService.Comment;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }
 
        private sealed class ClassificationWorker
        {
            private readonly List<RawSyntaxTag> _classificationSpans;

            public ClassificationWorker(List<RawSyntaxTag> classificationSpans)
            {
                _classificationSpans = classificationSpans;
            }

            private void AddClassification(TextSpan textSpan, RawSyntaxTagKind kind)
            {
                _classificationSpans.Add(new RawSyntaxTag(textSpan, kind));
            }

            private void AddClassification(SyntaxToken token, RawSyntaxTagKind kind)
            {
                if (token.Span.Length > 0)
                    AddClassification(token.Span, kind);
            }

            private void AddClassification(SyntaxTrivia trivia, RawSyntaxTagKind kind)
            {
                if (trivia.Span.Length > 0)
                    AddClassification(trivia.Span, kind);
            }

            public void ClassifyNode(SyntaxNode node)
            {
                var nodes = node.ChildNodesAndTokens();

                foreach (var syntaxNodeOrToken in nodes)
                    ClassifyNodeOrToken(syntaxNodeOrToken);
            }

            private void ClassifyNodeOrToken(SyntaxNodeOrToken nodeOrToken)
            {
                var asNode = nodeOrToken.AsNode();
                if (asNode != null)
                    ClassifyNode(asNode);
                else
                    ClassifyToken(nodeOrToken.AsToken());
            }

            private void ClassifyToken(SyntaxToken token)
            {
                var span = token.Span;
                if (span.IntersectsWith(span))
                {
                    var kind = GetClassificationForToken(token);
                    if (kind != null)
                        AddClassification(token, kind.Value);
                }

                foreach (var trivia in token.LeadingTrivia)
                    ClassifyTrivia(trivia);

                foreach (var trivia in token.TrailingTrivia)
                    ClassifyTrivia(trivia);
            }

            private void ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (trivia.Kind.IsComment())
                    AddClassification(trivia, RawSyntaxTagKind.Comment);
                else if (trivia.Structure != null)
                    ClassifyNode(trivia.Structure);
            }

            private RawSyntaxTagKind? GetClassificationForToken(SyntaxToken token)
            {
                if (token.Kind.IsKeyword())
                    return RawSyntaxTagKind.Keyword;

                if (token.Kind.IsPunctuation())
                    return RawSyntaxTagKind.Operator;

                if (token.Kind == SyntaxKind.IdentifierToken)
                    return RawSyntaxTagKind.Identifier;

                if ((token.Kind == SyntaxKind.StringLiteralToken))
                    return RawSyntaxTagKind.StringLiteral;

                if (token.Kind == SyntaxKind.NumericLiteralToken)
                    return RawSyntaxTagKind.NumberLiteral;

                return null;
            }
        }
    }
}