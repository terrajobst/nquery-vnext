using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Language.Symbols;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticClassifier : AsyncTagger<IClassificationTag,RawClassificationTag>
    {
        private readonly INQuerySemanticClassificationService _classificationService;
        private readonly INQueryDocument _document;

        public NQuerySemanticClassifier(INQuerySemanticClassificationService classificationService, INQueryDocument document)
        {
            _classificationService = classificationService;
            _document = document;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<RawClassificationTag>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var snapshot = _document.GetTextSnapshot(syntaxTree);

            var result = new List<RawClassificationTag>();
            var worker = new ClassificationWorker(semanticModel, result);
            worker.ClassifyNodeAndChildren(semanticModel.Compilation.SyntaxTree.Root);

            return Tuple.Create(snapshot, result.AsEnumerable());
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, RawClassificationTag rawTag)
        {
            var span = rawTag.TextSpan;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            var classification = GetClassification(rawTag.Symbol);
            var tag = new ClassificationTag(classification);
            var tagSpan = new TagSpan<IClassificationTag>(snapshotSpan, tag);
            return tagSpan;
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
                case SymbolKind.CommonTableExpression:
                    return _classificationService.CommonTableExpression;
                case SymbolKind.TableInstance:
                    return GetClassification(((TableInstanceSymbol)symbol).Table);
                case SymbolKind.ColumnInstance:
                    return _classificationService.Column;
                case SymbolKind.Function:
                    return _classificationService.Function;
                case SymbolKind.Aggregate:
                    return _classificationService.Aggregate;
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
 
        private sealed class ClassificationWorker
        {
            private readonly SemanticModel _semanticModel;
            private readonly List<RawClassificationTag> _tagSpans;

            public ClassificationWorker(SemanticModel semanticModel, List<RawClassificationTag> tagSpans)
            {
                _semanticModel = semanticModel;
                _tagSpans = tagSpans;
            }

            private void AddClassification(TextSpan textSpan, Symbol symbol)
            {
                if (symbol.Kind == SymbolKind.BadSymbol || symbol.Kind == SymbolKind.BadTable)
                    return;

                _tagSpans.Add(new RawClassificationTag(textSpan, symbol));
            }

            private void AddClassification(SyntaxNodeOrToken nodeOrToken, Symbol symbol)
            {
                if (nodeOrToken.Span.Length > 0)
                    AddClassification(nodeOrToken.Span, symbol);
            }

            public void ClassifyNodeAndChildren(SyntaxNode node)
            {
                ClassifyNode(node);

                var childNodes = node.ChildNodesAndTokens()
                                     .Where(n => n.IsNode)
                                     .Select(n => n.AsNode());

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

                AddClassification(context, symbol);
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

                AddClassification(node.TableName, tableInstanceSymbol.Table);

                if (node.Alias != null)
                    AddClassification(node.Alias.Identifier, tableInstanceSymbol);
            }

            private void ClassifyDerivedTableReference(DerivedTableReferenceSyntax node)
            {
                var tableInstanceSymbol = _semanticModel.GetDeclaredSymbol(node);
                if (tableInstanceSymbol != null)
                    AddClassification(node.Name, tableInstanceSymbol);
            }
        }
    }
}