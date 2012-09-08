using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticClassifier : ITagger<IClassificationTag>
    {
        private readonly INQuerySemanticClassificationService _classificationService;
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySemanticModelManager _semanticModelManager;

        public NQuerySemanticClassifier(INQuerySemanticClassificationService classificationService, ITextBuffer textBuffer, INQuerySemanticModelManager semanticModelManager)
        {
            _classificationService = classificationService;
            _textBuffer = textBuffer;
            _semanticModelManager = semanticModelManager;
            _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
        }

        private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs e)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            var eventArgs = new SnapshotSpanEventArgs(snapshotSpan);
            OnTagsChanged(eventArgs);
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<ITagSpan<IClassificationTag>>();

            if (spans.Any())
            {
                var snapshot = spans.First().Snapshot;
                var start = spans.Select(s => s.Start.Position).Min();
                var end = spans.Select(s => s.End.Position).Max();
                var span = new SnapshotSpan(snapshot, Span.FromBounds(start, end));

                var semanticModel = _semanticModelManager.SemanticModel;
                if (semanticModel != null)
                {
                    var worker = new ClassificationWorker(_classificationService, semanticModel, span, result);
                    worker.ClassifyNodeAndChildren(semanticModel.Compilation.SyntaxTree.Root);
                }
            }

            return result;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private sealed class ClassificationWorker
        {
            private readonly INQuerySemanticClassificationService _classificationService;
            private readonly SemanticModel _semanticModel;
            private readonly SnapshotSpan _snapshotSpan;
            private readonly TextSpan _span;
            private readonly List<ITagSpan<IClassificationTag>> _tagSpans;

            public ClassificationWorker(INQuerySemanticClassificationService classificationService, SemanticModel semanticModel, SnapshotSpan snapshotSpan, List<ITagSpan<IClassificationTag>> tagSpans)
            {
                _classificationService = classificationService;
                _semanticModel = semanticModel;
                _snapshotSpan = snapshotSpan;
                _span = new TextSpan(_snapshotSpan.Start, _snapshotSpan.Length);
                _tagSpans = tagSpans;
            }

            private void AddClassification(TextSpan textSpan, IClassificationType type)
            {
                var snapshot = _snapshotSpan.Snapshot;
                var span = new Span(textSpan.Start, textSpan.Length);
                var snapshotSpan = new SnapshotSpan(snapshot, span);
                var tag = new ClassificationTag(type);
                var tagSpan = new TagSpan<IClassificationTag>(snapshotSpan, tag);
                _tagSpans.Add(tagSpan);
            }

            private void AddClassification(SyntaxNodeOrToken nodeOrToken, IClassificationType type)
            {
                if (nodeOrToken.Span.Length > 0)
                    AddClassification(nodeOrToken.Span, type);
            }

            public void ClassifyNodeAndChildren(SyntaxNode node)
            {
                if (!node.FullSpan.OverlapsWith(_span))
                    return;

                ClassifyNode(node);

                var childNodes = node.ChildNodesAndTokens()
                                     .Where(n => n.IsNode)
                                     .Select(n => n.AsNode())
                                     .SkipWhile(n => !n.FullSpan.IntersectsWith(_span))
                                     .TakeWhile(n => n.FullSpan.IntersectsWith(_span));

                foreach (var childNode in childNodes)
                    ClassifyNodeAndChildren(childNode);
            }

            private void ClassifyNode(SyntaxNode node)
            {
                switch (node.Kind)
                {
                    case SyntaxKind.NameExpression:
                        ClassifyNameExpression((NameExpressionSyntax)node);
                        break;
                    case SyntaxKind.VariableExpression:
                        ClassifyVariableExpression((VariableExpressionSyntax)node);
                        break;
                    case SyntaxKind.FunctionInvocationExpression:
                        ClassifyFunctionInvocationExpression((FunctionInvocationExpressionSyntax)node);
                        break;
                    case SyntaxKind.PropertyAccessExpression:
                        ClassifyPropertyAccess((PropertyAccessExpressionSyntax)node);
                        break;
                    case SyntaxKind.MethodInvocationExpression:
                        ClassifyMethodInvocationExpression((MethodInvocationExpressionSyntax)node);
                        break;
                    case SyntaxKind.NamedTableReference:
                        ClassifyNamedTableReference((NamedTableReferenceSyntax)node);
                        break;
                    case SyntaxKind.DerivedTableReference:
                        ClassifyDerivedTableReference((DerivedTableReferenceSyntax)node);
                        break;
                }
            }

            private void ClassifyExpression(ExpressionSyntax node, SyntaxNodeOrToken context)
            {
                var symbol = _semanticModel.GetSymbol(node);
                if (symbol == null)
                    return;

                var classification = GetClassification(symbol);
                if (classification != null)
                    AddClassification(context, classification);
            }

            private void ClassifyNameExpression(NameExpressionSyntax node)
            {
                ClassifyExpression(node, node.Name);
            }

            private void ClassifyVariableExpression(VariableExpressionSyntax node)
            {
                ClassifyExpression(node, node);
            }

            private void ClassifyFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
            {
                ClassifyExpression(node, node.Name);
                ClassifyNode(node.ArgumentList);
            }

            private void ClassifyPropertyAccess(PropertyAccessExpressionSyntax node)
            {
                ClassifyNode(node.Target);
                ClassifyExpression(node, node.Name);
            }

            private void ClassifyMethodInvocationExpression(MethodInvocationExpressionSyntax node)
            {
                ClassifyNode(node.Target);
                ClassifyExpression(node, node.Name);
                ClassifyNode(node.ArgumentList);
            }

            private void ClassifyNamedTableReference(NamedTableReferenceSyntax node)
            {
                var tableInstanceSymbol = _semanticModel.GetDeclaredSymbol(node);
                if (tableInstanceSymbol == null)
                    return;

                var tableClassification = GetClassification(tableInstanceSymbol.Table);
                if (tableClassification != null)
                    AddClassification(node.TableName, tableClassification);

                if (node.Alias != null)
                {
                    var instanceClassification = GetClassification(tableInstanceSymbol);
                    if (instanceClassification != null)
                        AddClassification(node.Alias.Identifier, instanceClassification);
                }
            }

            private void ClassifyDerivedTableReference(DerivedTableReferenceSyntax node)
            {
                var tableInstanceSymbol = _semanticModel.GetDeclaredSymbol(node);
                if (tableInstanceSymbol == null)
                    return;

                var instanceClassification = GetClassification(tableInstanceSymbol);
                if (instanceClassification != null)
                    AddClassification(node.Name, instanceClassification);
            }

            private IClassificationType GetClassification(Symbol symbol)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.SchemaTable:
                        return _classificationService.SchemaTable;
                    case SymbolKind.Column:
                        return _classificationService.Column;
                    case SymbolKind.DerivedTable:
                        return _classificationService.DerivedTable;
                    case SymbolKind.TableInstance:
                        return GetClassification(((TableInstanceSymbol)symbol).Table);
                    case SymbolKind.ColumnInstance:
                        return _classificationService.Column;
                    case SymbolKind.Function:
                        return _classificationService.Function;
                    case SymbolKind.Variable:
                        return _classificationService.Variable;
                    case SymbolKind.Property:
                        return _classificationService.Property;
                    case SymbolKind.Method:
                        return _classificationService.Method;
                    default:
                        return null;
                }
            }
        }
    }
}