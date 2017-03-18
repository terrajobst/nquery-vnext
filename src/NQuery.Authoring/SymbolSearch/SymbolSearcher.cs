using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.SymbolSearch
{
    // These should be unit tests:
    //
    //  SELECT d.FullName
    //  FROM    (
    //              SELECT e.*,
    //                      e.FirstName + ' ' + e.LastName AS FullName
    //              FROM    Employees e
    //              ORDER BY FullName
    //          ) d
    //
    //  ----
    //
    //  WITH LondonEmps AS
    //  (
    //      SELECT e.FirstName
    //      FROM    Employees e
    //      HAVING City = 'London'
    //      ORDER BY FirstName
    //  )
    //  SELECT d.FirstName
    //  FROM LondonEmps d
    //
    //  ----
    //
    //  WITH LondonEmps(FirstName) AS
    //  (
    //      SELECT e.FirstName
    //      FROM    Employees e
    //      HAVING City = 'London'
    //      ORDER BY FirstName
    //  )
    //  SELECT d.FirstName
    //  FROM LondonEmps d

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
                   where IsMatch(s.Symbol, symbol) ||
                         IsMatch(symbol, s.Symbol)
                   select s;
        }

        private static bool IsMatch(Symbol symbol1, Symbol symbol2)
        {
            if (symbol1 == symbol2)
                return true;

            if (symbol1 is QueryColumnSymbol queryColumnSymbol && queryColumnSymbol.QueryOutput == symbol2)
                return true;

            if (symbol1 is TableColumnInstanceSymbol tci && tci.Column == symbol2)
                return true;

            if (symbol1 is TableColumnInstanceSymbol x && x.Column is QueryColumnSymbol y && y.QueryOutput == symbol2)
                return true;

            return false;
        }

        private static IEnumerable<SymbolSpan> GetSymbolSpans(SemanticModel semanticModel, SyntaxNode node)
        {
            switch (node)
            {
                case NameExpressionSyntax expression:
                {
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case PropertyAccessExpressionSyntax expression:
                {
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case MethodInvocationExpressionSyntax expression:
                {
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case FunctionInvocationExpressionSyntax expression:
                {
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case CountAllExpressionSyntax expression:
                {
                    var symbol = semanticModel.GetSymbol(expression);
                    yield return SymbolSpan.CreateReference(symbol, expression.Name.Span);
                    break;
                }
                case ExpressionSelectColumnSyntax selectColumn:
                {
                    if (selectColumn.Alias != null)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(selectColumn);
                        yield return SymbolSpan.CreateDefinition(symbol, selectColumn.Alias.Identifier.Span);
                    }
                    break;
                }
                case CommonTableExpressionSyntax commonTableExpression:
                {
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
                case DerivedTableReferenceSyntax derivedTable:
                {
                    var symbol = semanticModel.GetDeclaredSymbol(derivedTable);
                    yield return SymbolSpan.CreateDefinition(symbol, derivedTable.Name.Span);
                    break;
                }
                case NamedTableReferenceSyntax namedTable:
                {
                    var tableInstanceSymbol = semanticModel.GetDeclaredSymbol(namedTable);
                    if (tableInstanceSymbol != null)
                    {
                        yield return SymbolSpan.CreateReference(tableInstanceSymbol.Table, namedTable.TableName.Span);
                        if (namedTable.Alias != null)
                            yield return SymbolSpan.CreateDefinition(tableInstanceSymbol, namedTable.Alias.Identifier.Span);
                    }
                    break;
                }
            }
        }
    }
}