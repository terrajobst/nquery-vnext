using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Tagging.Implementation;

using NQuery.Language;
using NQuery.Language.Symbols;

namespace NQueryViewerActiproWpf
{
    internal sealed class NQuerySemanticClassifier : CollectionTagger<IClassificationTag>
    {
        private readonly INQueryClassificationTypes _classificationTypes;

        public NQuerySemanticClassifier(ICodeDocument document)
            : base(typeof(NQuerySemanticClassifier).Name, null, document, true)
        {
            _classificationTypes = document.Language.GetService<INQueryClassificationTypes>();

            var nqueryDocument = document as NQueryDocument;
            if (nqueryDocument != null)
            {
                nqueryDocument.SemanticModelChanged += NqueryDocumentOnSemanticModelChanged;
                UpdateTags();
            }
        }

        private void NqueryDocumentOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        private async void UpdateTags()
        {
            var snapshot = Document.CurrentSnapshot;
            var nqueryDocument = Document as NQueryDocument;
            var semanticData = nqueryDocument.GetSemanticData();
            if (semanticData == null)
                return;

            var tags = await ClassifyAsync(snapshot, semanticData.SemanticModel, _classificationTypes);

            using (CreateBatch())
            {
                Clear();
                foreach (var tag in tags)
                    Add(tag);
            }
        }

        private static Task<List<TagVersionRange<IClassificationTag>>> ClassifyAsync(ITextSnapshot snapshot, SemanticModel semanticModel, INQueryClassificationTypes classificationTypes)
        {
            return Task.Factory.StartNew(() =>
            {
                var result = new List<TagVersionRange<IClassificationTag>>();
                var worker = new ClassificationWorker(result, snapshot, semanticModel, classificationTypes);
                worker.ClassifyNodeAndChildren(semanticModel.Compilation.SyntaxTree.Root);
                return result;
            });
        }

        private sealed class ClassificationWorker
        {
            private readonly List<TagVersionRange<IClassificationTag>> _result;
            private readonly ITextSnapshot _snapshot;
            private readonly SemanticModel _semanticModel;
            private readonly INQueryClassificationTypes _classificationTypes;

            public ClassificationWorker(List<TagVersionRange<IClassificationTag>> result, ITextSnapshot snapshot, SemanticModel semanticModel, INQueryClassificationTypes classificationTypes)
            {
                _result = result;
                _snapshot = snapshot;
                _semanticModel = semanticModel;
                _classificationTypes = classificationTypes;
            }

            private void AddClassification(TextSpan textSpan, Symbol symbol)
            {
                if (symbol.Kind == SymbolKind.BadSymbol || symbol.Kind == SymbolKind.BadTable)
                    return;

                var textBuffer = _semanticModel.Compilation.SyntaxTree.TextBuffer;
                var range = textBuffer.ToSnapshotRange(_snapshot, textSpan);
                var classificationType = GetClassification(symbol);
                var classificationTag = new ClassificationTag(classificationType);
                var tagVersionRange = new TagVersionRange<IClassificationTag>(range, TextRangeTrackingModes.Default, classificationTag);
                _result.Add(tagVersionRange);
            }

            private IClassificationType GetClassification(Symbol symbol)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.SchemaTable:
                        return _classificationTypes.SchemaTable;
                    case SymbolKind.Column:
                        return _classificationTypes.Column;
                    case SymbolKind.DerivedTable:
                        return _classificationTypes.DerivedTable;
                    case SymbolKind.CommonTableExpression:
                        return _classificationTypes.CommonTableExpression;
                    case SymbolKind.TableInstance:
                        return GetClassification(((TableInstanceSymbol)symbol).Table);
                    case SymbolKind.ColumnInstance:
                        return _classificationTypes.Column;
                    case SymbolKind.Function:
                        return _classificationTypes.Function;
                    case SymbolKind.Aggregate:
                        return _classificationTypes.Aggregate;
                    case SymbolKind.Variable:
                        return _classificationTypes.Variable;
                    case SymbolKind.Property:
                        return _classificationTypes.Property;
                    case SymbolKind.Method:
                        return _classificationTypes.Method;
                    default:
                        return null;
                }
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
                    case SyntaxKind.CountAllExpression:
                        ClassifyCountAllExpression((CountAllExpressionSyntax)node);
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

            private void ClassifyCountAllExpression(CountAllExpressionSyntax node)
            {
                ClassifyExpression(node, node.Name);
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