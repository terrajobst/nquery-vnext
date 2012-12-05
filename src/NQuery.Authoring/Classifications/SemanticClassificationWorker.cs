using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Authoring.Classifications
{
    internal sealed class SemanticClassificationWorker
    {
        private readonly List<SemanticClassificationSpan> _result;
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _span;

        public SemanticClassificationWorker(List<SemanticClassificationSpan> result, SemanticModel semanticModel, TextSpan span)
        {
            _result = result;
            _semanticModel = semanticModel;
            _span = span;
        }

        private void AddClassification(TextSpan textSpan, Symbol symbol)
        {
            if (textSpan.Length == 0)
                return;

            var classification = GetClassification(symbol);
            if (classification == null)
                return;

            _result.Add(new SemanticClassificationSpan(textSpan, classification.Value));
        }

        private void AddClassification(SyntaxNodeOrToken nodeOrToken, Symbol symbol)
        {
            AddClassification(nodeOrToken.Span, symbol);
        }

        private static SemanticClassification? GetClassification(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.BadSymbol:
                case SymbolKind.BadTable:
                    return null;
                case SymbolKind.SchemaTable:
                    return SemanticClassification.SchemaTable;
                case SymbolKind.Column:
                    return SemanticClassification.Column;
                case SymbolKind.DerivedTable:
                    return SemanticClassification.DerivedTable;
                case SymbolKind.CommonTableExpression:
                    return SemanticClassification.CommonTableExpression;
                case SymbolKind.TableInstance:
                    return GetClassification(((TableInstanceSymbol)symbol).Table);
                case SymbolKind.TableColumnInstance:
                case SymbolKind.QueryColumnInstance:
                    return SemanticClassification.Column;
                case SymbolKind.Function:
                    return SemanticClassification.Function;
                case SymbolKind.Aggregate:
                    return SemanticClassification.Aggregate;
                case SymbolKind.Variable:
                    return SemanticClassification.Variable;
                case SymbolKind.Property:
                    return SemanticClassification.Property;
                case SymbolKind.Method:
                    return SemanticClassification.Method;
                default:
                    throw new ArgumentException("symbol");
            }
        }

        public void ClassifyNode(SyntaxNode node)
        {
            if (!node.FullSpan.OverlapsWith(_span))
                return;

            ClassifyNodeWithoutChildren(node);

            var nodes = node.ChildNodesAndTokens()
                            .Where(n => n.IsNode)
                            .SkipWhile(n => !n.FullSpan.IntersectsWith(_span))
                            .TakeWhile(n => n.FullSpan.IntersectsWith(_span))
                            .Select(n => n.AsNode());

            foreach (var childNode in nodes)
                ClassifyNode(childNode);
        }

        private void ClassifyNodeWithoutChildren(SyntaxNode node)
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
                case SyntaxKind.WildcardSelectColumn:
                    ClassifyWildcardSelectColumn((WildcardSelectColumnSyntax)node);
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
            ClassifyNodeWithoutChildren(node.ArgumentList);
        }

        private void ClassifyCountAllExpression(CountAllExpressionSyntax node)
        {
            ClassifyExpression(node, node.Name);
        }

        private void ClassifyPropertyAccess(PropertyAccessExpressionSyntax node)
        {
            ClassifyNodeWithoutChildren(node.Target);
            ClassifyExpression(node, node.Name);
        }

        private void ClassifyMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            ClassifyNodeWithoutChildren(node.Target);
            ClassifyExpression(node, node.Name);
            ClassifyNodeWithoutChildren(node.ArgumentList);
        }

        private void ClassifyWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            var tableInstanceSymbol = _semanticModel.GetTableInstance(node);
            if (tableInstanceSymbol == null)
                return;

            AddClassification(node.TableName, tableInstanceSymbol);
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