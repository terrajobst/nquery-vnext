using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    internal sealed class NQuerySyntacticClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;

        public NQuerySyntacticClassifier(ICodeDocument document)
            : base(typeof(NQuerySyntacticClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();
            document.TextChanged += DocumentOnTextChanged;
            UpdateTags();
        }

        private void DocumentOnTextChanged(object sender, TextSnapshotChangedEventArgs e)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var snapshot = Document.CurrentSnapshot;
            var syntaxTree = await ParseSyntaxTreeAsync(snapshot);
            var tags = await ClassifyTreeAsync(snapshot, syntaxTree, _classificationTypes);

            using (CreateBatch())
            {
                Clear();
                foreach (var tag in tags)
                    Add(tag);
            }
        }

        private static Task<SyntaxTree> ParseSyntaxTreeAsync(ITextSnapshot snapshot)
        {
            return Task.Factory.StartNew(() => SyntaxTree.ParseQuery(snapshot.Text));
        }

        private static Task<List<TagVersionRange<IClassificationTag>>> ClassifyTreeAsync(ITextSnapshot snapshot, SyntaxTree syntaxTree, INQueryClassificationTypes classificationTypes)
        {
            return Task.Factory.StartNew(() => ClassifyTree(snapshot, syntaxTree, classificationTypes));
        }

        private static List<TagVersionRange<IClassificationTag>> ClassifyTree(ITextSnapshot snapshot, SyntaxTree syntaxTree, INQueryClassificationTypes classificationTypes)
        {
            var result = new List<TagVersionRange<IClassificationTag>>();
            var worker = new ClassificationWorker(snapshot, result, classificationTypes);
            worker.ClassifyNode(syntaxTree.Root);
            return result;
        }

        private sealed class ClassificationWorker
        {
            private readonly ITextSnapshot _snapshot;
            private readonly List<TagVersionRange<IClassificationTag>> _result;
            private readonly INQueryClassificationTypes _classificationTypes;

            public ClassificationWorker(ITextSnapshot snapshot, List<TagVersionRange<IClassificationTag>> result, INQueryClassificationTypes classificationTypes)
            {
                _snapshot = snapshot;
                _result = result;
                _classificationTypes = classificationTypes;
            }

            public void ClassifyNode(SyntaxNode root)
            {
                foreach (var nodeOrToken in root.ChildNodesAndTokens())
                {
                    if (nodeOrToken.IsToken)
                        ClassifyToken(nodeOrToken.AsToken());
                    else
                        ClassifyNode(nodeOrToken.AsNode());
                }
            }

            private void ClassifyToken(SyntaxToken token)
            {
                ClassifyTrivia(token.LeadingTrivia);

                if (token.Span.Length > 0)
                {
                    if (token.Kind.IsIdentifierOrKeyword())
                    {
                        var type = token.Kind.IsKeyword() ? _classificationTypes.Keyword : _classificationTypes.Identifier;
                        AddClassification(token.Span, type, token.Parent.SyntaxTree);
                    }
                    else if (token.Kind.IsPunctuation())
                    {
                        AddClassification(token.Span, _classificationTypes.Punctuation, token.Parent.SyntaxTree);
                    }
                    else if (token.Kind.IsLiteral())
                    {
                        switch (token.Kind)
                        {
                            case SyntaxKind.StringLiteralToken:
                                AddClassification(token.Span, _classificationTypes.StringLiteral, token.Parent.SyntaxTree);
                                break;
                            case SyntaxKind.NumericLiteralToken:
                                AddClassification(token.Span, _classificationTypes.NumberLiteral, token.Parent.SyntaxTree);
                                break;
                            case SyntaxKind.DateLiteralToken:
                                AddClassification(token.Span, _classificationTypes.StringLiteral, token.Parent.SyntaxTree);
                                break;
                        }
                    }
                }

                ClassifyTrivia(token.TrailingTrivia);
            }

            private void AddClassification(TextSpan span, IClassificationType classificationType, SyntaxTree syntaxTree)
            {
                var range = syntaxTree.TextBuffer.ToSnapshotRange(_snapshot, span);
                var classificationTag = new ClassificationTag(classificationType);
                var tagVersionRange = new TagVersionRange<IClassificationTag>(range, TextRangeTrackingModes.Default, classificationTag);
                _result.Add(tagVersionRange);
            }

            private void ClassifyTrivia(IEnumerable<SyntaxTrivia> triviaList)
            {
                foreach (var trivia in triviaList)
                    ClassifyTrivia(trivia);
            }

            private void ClassifyTrivia(SyntaxTrivia trivia)
            {
                if (trivia.Kind.IsComment())
                {
                    AddClassification(trivia.Span, _classificationTypes.Comment, trivia.Parent.Parent.SyntaxTree);
                }
                else if (trivia.Structure != null)
                {
                    ClassifyNode(trivia.Structure);
                }
            }
        }
    }
}