using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private BoundQuery BindQuery(QuerySyntax node)
        {
            return Bind(node, BindQueryInternal);
        }

        private BoundQuery BindQueryInternal(QuerySyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ExceptQuery:
                    return BindExceptQuery((ExceptQuerySyntax)node);

                case SyntaxKind.UnionQuery:
                    return BindUnionQuery((UnionQuerySyntax)node);

                case SyntaxKind.IntersectQuery:
                    return BindIntersectQuery((IntersectQuerySyntax)node);

                case SyntaxKind.OrderedQuery:
                    return BindOrderedQuery((OrderedQuerySyntax)node);

                case SyntaxKind.ParenthesizedQuery:
                    return BindParenthesizedQuery((ParenthesizedQuerySyntax)node);

                case SyntaxKind.CommonTableExpressionQuery:
                    return BindCommonTableExpressionQuery((CommonTableExpressionQuerySyntax)node);

                case SyntaxKind.SelectQuery:
                    return BindSelectQuery((SelectQuerySyntax)node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundQuery BindExceptQuery(ExceptQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);

            return new BoundCombinedQuery(left, BoundQueryCombinator.Except, right);
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var combinator = node.AllKeyword == null
                                 ? BoundQueryCombinator.Union
                                 : BoundQueryCombinator.UnionAll;

            return new BoundCombinedQuery(left, combinator, right);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            return new BoundCombinedQuery(left, BoundQueryCombinator.Intersect, right);
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            var query = BindQuery(node.Query);

            throw new NotImplementedException();
        }

        private BoundQuery BindParenthesizedQuery(ParenthesizedQuerySyntax node)
        {
            return BindQuery(node.Query);
        }

        private BoundQuery BindCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node)
        {
            var fromClause = BindFromClause(node.FromClause);

            var bindingContextWithTables = fromClause == null
                                               ? _bindingContext
                                               : new AdditionalSymbolsBindingContext(_bindingContext, fromClause.GetDeclaredTableInstances());

            var binder = GetBinder(bindingContextWithTables);

            var whereClause = binder.BindWhereClause(node.WhereClause);
            var selectColumns = binder.BindSelectColumns(node.SelectClause.Columns);

            if (node.GroupByClause != null)
            {
                // TODO: Bind GroupByClause            
            }

            var havingClause = binder.BindHavingClause(node.HavingClause);

            return new BoundSelectQuery(selectColumns, fromClause, whereClause, havingClause);
        }

        private IList<BoundSelectColumn> BindSelectColumns(IEnumerable<SelectColumnSyntax> nodes)
        {
            var result = new List<BoundSelectColumn>();
            foreach (var node in nodes)
            {
                switch (node.Kind)
                {
                    case SyntaxKind.ExpressionSelectColumn:
                        var boundColumn = BindExpressionSelectColumn((ExpressionSelectColumnSyntax)node);
                        result.Add(boundColumn);
                        break;

                    case SyntaxKind.WildcardSelectColumn:
                        var boundColumns = BindWildcardSelectColumn((WildcardSelectColumnSyntax)node);
                        result.AddRange(boundColumns);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Unknown column kind {0}.", node.Kind), "nodes");
                }
            }

            return result;
        }

        private BoundSelectColumn BindExpressionSelectColumn(ExpressionSelectColumnSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var name = node.Alias != null
                           ? node.Alias.Identifier.ValueText
                           : InferColumnName(expression);
            return new BoundSelectColumn(expression, name);
        }

        private static string InferColumnName(BoundExpression expression)
        {
            var nameExpression = expression as BoundNameExpression;
            return nameExpression != null ? nameExpression.Symbol.Name : null;
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            return node.TableName != null
                       ? BindWildcardSelectColumnForTable(node.TableName.Value)
                       : BindWildcardSelectColumnForAllTables();
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForTable(SyntaxToken tableName)
        {
            var symbols = LookupTableInstance(tableName).ToArray();

            if (symbols.Length == 0)
            {
                // TODO: Report unresolved
                return Enumerable.Empty<BoundSelectColumn>();
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match
            }

            var tableInstance = symbols[0];
            return BindSelectColumnFromColumns(tableInstance);
        }

        private static IEnumerable<BoundSelectColumn> BindSelectColumnFromColumns(TableInstanceSymbol tableInstance)
        {
            return from columnInstance in tableInstance.ColumnInstances
                   let expression = new BoundNameExpression(columnInstance, Enumerable.Empty<Symbol>())
                   select new BoundSelectColumn(expression, columnInstance.Name);
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForAllTables()
        {
            var symbols = LookupTableInstances().ToArray();

            return from tableInstance in symbols
                   from column in BindSelectColumnFromColumns(tableInstance)
                   select column;
        }

        private BoundTableReference BindFromClause(FromClauseSyntax node)
        {
            if (node == null)
                return null;

            BoundTableReference lastTableReference = null;

            foreach (var tableReference in node.TableReferences)
            {
                var boundTableReference = BindTableReference(tableReference);

                if (lastTableReference == null)
                {
                    lastTableReference = boundTableReference;
                }
                else
                {
                    lastTableReference = new BoundJoinedTableReference(BoundJoinType.InnerJoin, lastTableReference, boundTableReference, null);
                }
            }

            return lastTableReference;
        }

        private BoundExpression BindWhereClause(WhereClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundWhereClause = BindExpression(node.Predicate);
            // TODO: Ensure where evaluates to boolean
            return boundWhereClause;
        }

        private BoundExpression BindHavingClause(HavingClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundHavingClause = BindExpression(node.Predicate);
            // TODO: Ensure having evaluates to boolean

            return boundHavingClause;
        }
    }
}