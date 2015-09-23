using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.SymbolSearch
{
    public static class SymbolSearcher
    {
        public static SymbolSpan? FindSymbol(this SemanticModel semanticModel, int position)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            var syntaxTree = semanticModel.SyntaxTree;
            return syntaxTree.Root.FindNodes(position)
                                  .SelectMany(n => GetSymbolSpans(semanticModel, n))
                                  .Where(s => s.Span.ContainsOrTouches(position))
                                  .Select(s => s).Cast<SymbolSpan?>().FirstOrDefault();
        }

        public static IEnumerable<SymbolSpan> FindUsages(this SemanticModel semanticModel, Symbol symbol)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var syntaxTree = semanticModel.SyntaxTree;

            return from n in syntaxTree.Root.DescendantNodes()
                   from s in GetSymbolSpans(semanticModel, n)
                   where s.Symbol == symbol
                   select s;
        }

        private static IEnumerable<SymbolSpan> GetSymbolSpans(SemanticModel semanticModel, SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.NameExpression:
                {
                    var expression = (NameExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case SyntaxKind.PropertyAccessExpression:
                {
                    var expression = (PropertyAccessExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case SyntaxKind.MethodInvocationExpression:
                {
                    var expression = (MethodInvocationExpressionSyntax)node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case SyntaxKind.FunctionInvocationExpression:
                {
                    var expression = (FunctionInvocationExpressionSyntax)node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case SyntaxKind.CountAllExpression:
                {
                    var countAllExpression = (CountAllExpressionSyntax)node;
                    var symbol = semanticModel.GetSymbol(countAllExpression);
                    yield return SymbolSpan.CreateReference(symbol, countAllExpression.Name.Span);
                    break;
                }
                case SyntaxKind.ExpressionSelectColumn:
                {
                    var selectColumn = (ExpressionSelectColumnSyntax)node;
                    if (selectColumn.Alias != null)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(selectColumn);
                        yield return SymbolSpan.CreateDefinition(symbol, selectColumn.Alias.Identifier.Span);
                    }
                    break;
                }
                case SyntaxKind.CommonTableExpression:
                {
                    var commonTableExpression = (CommonTableExpressionSyntax)node;
                    var symbol = semanticModel.GetDeclaredSymbol(commonTableExpression);
                    yield return SymbolSpan.CreateDefinition(symbol, commonTableExpression.Name.Span);

                    if (commonTableExpression.ColumnNameList != null)
                    {
                        foreach (var columnName in commonTableExpression.ColumnNameList.ColumnNames)
                        {
                            var columnSymbol = semanticModel.GetDeclaredSymbol(columnName);
                            if (columnSymbol != null)
                                yield return SymbolSpan.CreateDefinition(columnSymbol, columnName.Span);
                        }
                    }
                    break;
                }
                case SyntaxKind.DerivedTableReference:
                {
                    var derivedTable = (DerivedTableReferenceSyntax)node;
                    var symbol = semanticModel.GetDeclaredSymbol(derivedTable);
                    yield return SymbolSpan.CreateDefinition(symbol, derivedTable.Name.Span);
                    break;
                }
                case SyntaxKind.NamedTableReference:
                {
                    var namedTable = (NamedTableReferenceSyntax)node;
                    var tableInstanceSymbol = semanticModel.GetDeclaredSymbol(namedTable);
                    if (tableInstanceSymbol != null)
                    {
                        yield return SymbolSpan.CreateReference(tableInstanceSymbol.Table, namedTable.TableName.Span);
                        if (namedTable.Alias != null)
                            yield return SymbolSpan.CreateDefinition(tableInstanceSymbol, namedTable.Alias.Identifier.Span);
                    }
                    break;
                }
                case SyntaxKind.OrderByColumn:
                {
                    var orderByColumn = (OrderByColumnSyntax)node;
                    var isOrdinal = orderByColumn.ColumnSelector is LiteralExpressionSyntax;
                    var columnInstanceSymbol = semanticModel.GetSymbol(orderByColumn);
                    if (isOrdinal && columnInstanceSymbol != null)
                        yield return SymbolSpan.CreateReference(columnInstanceSymbol, orderByColumn.ColumnSelector.Span);
                    break;
                }
            }
        }
    }
}