using System;
using System.Collections.Generic;
using System.Linq;

using NQuery;
using NQuery.Symbols;
using NQuery.Symbols.Aggregation;
using NQuery.Syntax;

namespace NQueryDesigner.Authoring
{
    public static class QueryDesigerExtensions
    {
        public static QueryInfo AnalyzeQuery(this SemanticModel semanticModel)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;

            var selectionInfos = new List<SelectionInfo>();
            var tables = new List<TableInstanceSymbol>();
            var orderByQuery = syntaxTree.Root.Root as OrderedQuerySyntax;
            var selectQuery = orderByQuery != null
                                ? orderByQuery.Query as SelectQuerySyntax
                                : syntaxTree.Root.Root as SelectQuerySyntax;

            if (selectQuery != null)
            {
                var orderedColumn = GetSortOrders(semanticModel, orderByQuery);

                foreach (var columnSyntax in selectQuery.SelectClause.Columns)
                {
                    var wildcard = columnSyntax as WildcardSelectColumnSyntax;
                    if (wildcard != null)
                    {
                        var queryColumns = semanticModel.GetDeclaredSymbols(wildcard);
                        var tableColumns = semanticModel.GetColumnInstances(wildcard);
                        var columnPairs = queryColumns.Zip(tableColumns, Tuple.Create);

                        foreach (var columnPair in columnPairs)
                        {
                            var queryColumn = columnPair.Item1;
                            var tableColumn = columnPair.Item2;

                            QuerySortOrder sortOrder;
                            orderedColumn.TryGetValue(queryColumn, out sortOrder);

                            AddSelectionInfoForColumn(selectionInfos, tableColumn, null, null, sortOrder);
                        }
                    }
                    else
                    {
                        var expressionSyntax = (ExpressionSelectColumnSyntax)columnSyntax;
                        var expression = expressionSyntax.Expression;

                        var aggregate = semanticModel.GetSymbol(expression) as AggregateSymbol;
                        if (aggregate != null)
                        {
                            var invocationSyntax = (FunctionInvocationExpressionSyntax) expression;
                            expression = invocationSyntax.ArgumentList.Arguments.First();
                        }

                        var queryColumn = semanticModel.GetDeclaredSymbol(expressionSyntax);
                        var tableColumn = semanticModel.GetSymbol(expression) as TableColumnInstanceSymbol;

                        QuerySortOrder sortOrder;
                        orderedColumn.TryGetValue(queryColumn, out sortOrder);

                        if (tableColumn != null)
                        {
                            AddSelectionInfoForColumn(selectionInfos, tableColumn, expressionSyntax.Alias, aggregate, sortOrder);
                        }
                        else
                        {
                            AddSelectionInfoForExpression(selectionInfos, expression, expressionSyntax.Alias, aggregate, sortOrder);
                        }
                    }
                }

                if (selectQuery.FromClause != null)
                {
                    foreach (var tableReferenceSyntax in selectQuery.FromClause.TableReferences)
                    {
                        tables.AddRange(semanticModel.GetDeclaredSymbols(tableReferenceSyntax));
                    }
                }
            }

            return new QueryInfo(selectionInfos, tables);
        }

        private static Dictionary<QueryColumnInstanceSymbol, QuerySortOrder> GetSortOrders(SemanticModel semanticModel, OrderedQuerySyntax orderedQuerySyntax)
        {
            var result = new Dictionary<QueryColumnInstanceSymbol, QuerySortOrder>();

            if (orderedQuerySyntax != null)
            {
                foreach (var column in orderedQuerySyntax.Columns)
                {
                    var queryColumn = semanticModel.GetSymbol(column);
                    if (queryColumn == null)
                        continue;

                    var sortOrder = column.Modifier == null || column.Modifier.Kind == SyntaxKind.AscKeyword
                        ? QuerySortOrder.Ascending
                        : QuerySortOrder.Descending;

                    result.Add(queryColumn, sortOrder);
                }
            }

            return result;
        }

        private static void AddSelectionInfoForColumn(ICollection<SelectionInfo> receiver, TableColumnInstanceSymbol symbol, AliasSyntax alias, AggregateSymbol aggregate, QuerySortOrder sortOrder)
        {
            var columnName = symbol.Column.Name;
            var aliasName = GetAliasName(alias);
            var tableName = GetTableName(symbol.TableInstance);
            var aggregateName = GetAggregateName(aggregate);
            var info = new SelectionInfo(columnName, aliasName, tableName, aggregateName, sortOrder);
            receiver.Add(info);
        }

        private static void AddSelectionInfoForExpression(ICollection<SelectionInfo> receiver, ExpressionSyntax expression, AliasSyntax alias, AggregateSymbol aggregate, QuerySortOrder sortOrder)
        {
            var columnName = GetColumnName(expression);
            var aliasName = GetAliasName(alias);
            var aggregateName = GetAggregateName(aggregate);
            var info = new SelectionInfo(columnName, aliasName, null, aggregateName, sortOrder);
            receiver.Add(info);
        }

        private static string GetColumnName(ExpressionSyntax expression)
        {
            var syntaxTree = expression.SyntaxTree;
            var text = syntaxTree.Text.GetText(expression.Span);
            return text;
        }

        private static string GetAliasName(AliasSyntax alias)
        {
            return alias == null ? null : alias.Identifier.ValueText;
        }

        private static string GetTableName(TableInstanceSymbol symbol)
        {
            return symbol.Name == symbol.Table.Name
                ? symbol.Name
                : string.Format("{0} ({1})", symbol.Name, symbol.Table.Name);
        }

        private static string GetAggregateName(AggregateSymbol aggregate)
        {
            return aggregate == null ? null : aggregate.Name.ToLower();
        }
    }
}