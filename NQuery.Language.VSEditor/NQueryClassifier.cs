using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryClassifier : IClassifier
    {
        private readonly IStandardClassificationService _classificationService;
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;

        public NQueryClassifier(IStandardClassificationService classificationService, ITextBuffer textBuffer, INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _classificationService = classificationService;
            _textBuffer = textBuffer;
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnClassificationChanged(new ClassificationChangedEventArgs(snapshotSpan));
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var classificationSpans = new List<ClassificationSpan>();
            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            if (syntaxTree != null)
            {
                var worker = new ClassificationWorker(_classificationService, span, classificationSpans);
                worker.ClassifyNode(syntaxTree.Root);
            }
            return classificationSpans;
        }

        private void OnClassificationChanged(ClassificationChangedEventArgs e)
        {
            var handler = ClassificationChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        private sealed class ClassificationWorker
        {
            private readonly IStandardClassificationService _classificationService;
            private readonly List<ClassificationSpan> _classificationSpans;
            private readonly SnapshotSpan _snapshotSpan;
            private readonly TextSpan _span;

            public ClassificationWorker(IStandardClassificationService classificationService, SnapshotSpan snapshotSpan, List<ClassificationSpan> classificationSpans)
            {
                _classificationService = classificationService;
                _snapshotSpan = snapshotSpan;
                _classificationSpans = classificationSpans;
                _span = new TextSpan(_snapshotSpan.Start, _snapshotSpan.Length);
            }

            private void AddClassification(TextSpan textSpan, IClassificationType type)
            {
                var snapshot = _snapshotSpan.Snapshot;
                var span = new Span(textSpan.Start, textSpan.Length);
                var snapshotSpan = new SnapshotSpan(snapshot, span);
                var classificationSpan = new ClassificationSpan(snapshotSpan, type);
                _classificationSpans.Add(classificationSpan);
            }

            private void AddClassification(SyntaxToken token, IClassificationType type)
            {
                if (token.Span.Length > 0)
                    AddClassification(token.Span, type);
            }

            private void AddClassification(SyntaxTrivia trivia, IClassificationType type)
            {
                if (trivia.Span.Length > 0)
                    AddClassification(trivia.Span, type);
            }

            public void ClassifyNode(SyntaxNode node)
            {
                if (!node.FullSpan.OverlapsWith(_span))
                    return;

                var nodes = node.GetChildren()
                                .SkipWhile(n => !n.FullSpan.IntersectsWith(_span))
                                .TakeWhile(n => n.FullSpan.IntersectsWith(_span));

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
                    var type = GetClassificationForToken(token);
                    if (type != null)
                        AddClassification(token, type);
                }

                foreach (var trivia in token.LeadingTrivia)
                    ClassifyTrivia(trivia);

                foreach (var trivia in token.TrailingTrivia)
                    ClassifyTrivia(trivia);
            }

            private void ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (trivia.Kind.IsComment())
                    AddClassification(trivia, _classificationService.Comment);
                else if (trivia.Structure != null)
                    ClassifyNode(trivia.Structure);
            }

            private IClassificationType GetClassificationForToken(SyntaxToken token)
            {
                if (token.Kind.IsKeyword())
                    return _classificationService.Keyword;

                if (token.Kind.IsPunctuation())
                    return _classificationService.Operator;

                if (token.Kind == SyntaxKind.IdentifierToken)
                    return _classificationService.Identifier;

                if ((token.Kind == SyntaxKind.StringLiteralToken))
                    return _classificationService.StringLiteral;

                if (token.Kind == SyntaxKind.NumericLiteralToken)
                    return _classificationService.NumberLiteral;

                return null;
            }
        }
    }
}