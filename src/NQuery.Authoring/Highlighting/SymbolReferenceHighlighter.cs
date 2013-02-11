using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Highlighting
{
    internal sealed class SymbolReferenceHighlighter : IHighlighter
    {
        public IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindToken(position);
            var touchedSymbol = (from p in token.Parent.AncestorsAndSelf()
                                 from t in GetSymbols(semanticModel, p)
                                 where t != null
                                 let span = t.Item2
                                 where span.ContainsOrTouches(position)
                                 select t.Item1).FirstOrDefault();

            if (touchedSymbol == null)
                return Enumerable.Empty<TextSpan>();

            return from n in syntaxTree.Root.DescendantNodes()
                   from t in GetSymbols(semanticModel, n)
                   where t != null && t.Item1 == touchedSymbol
                   select t.Item2;
        }

        private static IEnumerable<Tuple<Symbol, TextSpan>> GetSymbols(SemanticModel semanticModel, SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.NameExpression:
                    var nameExpression = (NameExpressionSyntax) node;
                    yield return Tuple.Create(semanticModel.GetSymbol(nameExpression), nameExpression.Name.Span);
                    break;
                case SyntaxKind.PropertyAccessExpression:
                    var propertExpression = (PropertyAccessExpressionSyntax) node;
                    yield return Tuple.Create(semanticModel.GetSymbol(propertExpression), propertExpression.Name.Span);
                    break;
                case SyntaxKind.MethodInvocationExpression:
                    var methodExpression = (MethodInvocationExpressionSyntax) node;
                    yield return Tuple.Create(semanticModel.GetSymbol(methodExpression), methodExpression.Name.Span);
                    break;
                case SyntaxKind.FunctionInvocationExpression:
                    var functionExpression = (FunctionInvocationExpressionSyntax) node;
                    yield return Tuple.Create(semanticModel.GetSymbol(functionExpression), functionExpression.Name.Span);
                    break;
                case SyntaxKind.CountAllExpression:
                    var countAllExpression = (CountAllExpressionSyntax) node;
                    yield return Tuple.Create(semanticModel.GetSymbol(countAllExpression), countAllExpression.Name.Span);
                    break;
                case SyntaxKind.ExpressionSelectColumn:
                    var selectColumn = (ExpressionSelectColumnSyntax) node;
                    if (selectColumn.Alias != null)
                        yield return Tuple.Create<Symbol, TextSpan>(semanticModel.GetDeclaredSymbol(selectColumn), selectColumn.Alias.Identifier.Span);
                    break;
                case SyntaxKind.CommonTableExpression:
                    // TODO: We should also support CTE column lists.
                    var commonTableExpression = (CommonTableExpressionSyntax) node;
                    yield return Tuple.Create<Symbol, TextSpan>(semanticModel.GetDeclaredSymbol(commonTableExpression), commonTableExpression.Name.Span);
                    break;
                case SyntaxKind.DerivedTableReference:
                    var derivedTable = (DerivedTableReferenceSyntax) node;
                    yield return Tuple.Create<Symbol, TextSpan>(semanticModel.GetDeclaredSymbol(derivedTable), derivedTable.Name.Span);
                    break;
                case SyntaxKind.NamedTableReference:
                    var namedTable = (NamedTableReferenceSyntax) node;
                    var tableInstanceSymbol = semanticModel.GetDeclaredSymbol(namedTable);
                    yield return Tuple.Create<Symbol, TextSpan>(tableInstanceSymbol.Table, namedTable.TableName.Span);
                    if (namedTable.Alias != null)
                        yield return Tuple.Create<Symbol, TextSpan>(tableInstanceSymbol, namedTable.Alias.Identifier.Span);
                    break;
            }
        }
    }
}