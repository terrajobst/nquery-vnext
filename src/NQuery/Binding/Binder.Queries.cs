using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    partial class Binder
    {
        private static bool IsRecursive(CommonTableExpressionSyntax commonTableExpression)
        {
            return IsRecursive(commonTableExpression, commonTableExpression.Query);
        }

        private static bool IsRecursive(CommonTableExpressionSyntax commonTableExpression, QuerySyntax query)
        {
            return query.DescendantNodes().OfType<NamedTableReferenceSyntax>().Any(n => n.TableName.Matches(commonTableExpression.Name.ValueText));
        }

        private static string InferColumnName(BoundExpression expression)
        {
            var nameExpression = expression as BoundNameExpression;
            return nameExpression != null ? nameExpression.Symbol.Name : null;
        }

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
            // TODO: Check column count
            // TODO: Ensure all types are identical, if not we need to insert conversion operators
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            return new BoundCombinedQuery(left, BoundQueryCombinator.Except, right);
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            // TODO: Check column count
            // TODO: Ensure all types are identical, if not we need to insert conversion operators
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var combinator = node.AllKeyword == null
                                 ? BoundQueryCombinator.Union
                                 : BoundQueryCombinator.UnionAll;
            return new BoundCombinedQuery(left, combinator, right);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            // TODO: Check column count
            // TODO: Ensure all types are identical, if not we need to insert conversion operators
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            return new BoundCombinedQuery(left, BoundQueryCombinator.Intersect, right);
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            // TODO: We need to verify a few things here.
            //
            // SQL's semantics for ORDER BY are kina weird.
            //
            // (1) The binding context of the ORDER BY includes everything that the first, inner most
            //     SELECT query has, plus all defined output columns.
            //
            // (2) Of course, if the first SELECT query is grouped or aggregated, the values used
            //     in ORDER BY are subject to the usual constraints.
            //
            // (3) A literal integer expression in ORDER BY denotes the one-based output column.
            //     Any other literal value is treated as an expression.
            //
            // (4) Modulo numeric output column references (3), a constant expression will
            //     generate the error ('A constant expression was encountered in the ORDER BY list').
            //     Note this covers literals as well binary/unary exressions consisting of only literals.
            //
            // (5) ORDER BY cannot appear in subselect expressions, derived tables or common table
            //     expression, unless TOP is also specified.

            var query = BindQuery(node.Query);

            throw new NotImplementedException();
        }

        private BoundQuery BindParenthesizedQuery(ParenthesizedQuerySyntax node)
        {
            return BindQuery(node.Query);
        }

        private BoundQuery BindCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node)
        {
            // Each CTE has access to all tables plus any CTE specified previously.
            //
            // This means each CTE will produce a new binder that will contain the
            // introduced table symbol.
            //
            // We will also verify that there are no duplicate table names. 

            var currentBinder = this;
            var uniqueTableNames = new HashSet<string>();

            var boundCommonTableExpressions = new List<BoundCommonTableExpression>();

            foreach (var commonTableExpression in node.CommonTableExpressions)
            {
                var boundCommonTableExpression = currentBinder.BindCommonTableExpression(commonTableExpression);
                boundCommonTableExpressions.Add(boundCommonTableExpression);

                var tableSymbol = boundCommonTableExpression.TableSymbol;

                if (!uniqueTableNames.Add(tableSymbol.Name))
                    _diagnostics.ReportCteHasDuplicateTableName(commonTableExpression.Name);

                currentBinder = currentBinder.CreateLocalBinder(tableSymbol);

            }

            var boundQuery = currentBinder.BindQuery(node.Query);

            return new BoundCommonTableExpressionQuery(boundCommonTableExpressions, boundQuery);
        }

        private BoundCommonTableExpression BindCommonTableExpression(CommonTableExpressionSyntax commonTableExpression)
        {
            return Bind(commonTableExpression, BindCommonTableExpressionInternal);
        }

        private BoundCommonTableExpression BindCommonTableExpressionInternal(CommonTableExpressionSyntax commonTableExpression)
        {
            var isRecursive = IsRecursive(commonTableExpression);
            return isRecursive
                       ? BindCommonTableExpressionRecursive(commonTableExpression)
                       : BindCommonTableExpressionNonRecursive(commonTableExpression);
        }

        private BoundCommonTableExpression BindCommonTableExpressionNonRecursive(CommonTableExpressionSyntax commonTableExpression)
        {
            // First let's bind the query.

            var boundQuery = BindQuery(commonTableExpression.Query);

            // Now let's figure out the column names we want to give result.

            var specifiedColumnNames = commonTableExpression.ColumnNameList == null
                                           ? null
                                           : commonTableExpression.ColumnNameList.ColumnNames;

            var queryColumns = boundQuery.SelectColumns;

            if (specifiedColumnNames == null)
            {
                // If the CTE doesn't have a column list, the query must have names for all columns.

                for (var i = 0; i < queryColumns.Count; i++)
                {
                    if (string.IsNullOrEmpty(queryColumns[i].Name))
                        _diagnostics.ReportNoColumnAliasSpecified(commonTableExpression.Name, i);
                }
            }
            else
            {
                // If the CTE has a column list we need to make sure the number of 
                // names matches the number of columns in the underlying query.

                var specifiedCount = specifiedColumnNames.Count;
                var actualCount = queryColumns.Count;

                if (actualCount > specifiedCount)
                {
                    _diagnostics.ReportCteHasMoreColumnsThanSpecified(commonTableExpression.Name);
                }
                else if (actualCount < specifiedCount)
                {
                    _diagnostics.ReportCteHasFewerColumnsThanSpecified(commonTableExpression.Name);
                }
            }

            // Given the names let's construct the list of columns.
            //
            // We need to make sure that we produce a sensible result even if the
            // syntax is slightly inconsistent:
            // 
            // (1) The number of columns should neither exceed an explicit column list
            //     nor the number of columns provided by the underlying query.
            //
            // (2) The column list shouldn't contain any columns without a name.
            //
            // (3) The column list shouldn't contain duplicate names.

            var columnCount = specifiedColumnNames == null
                                  ? queryColumns.Count
                                  : Math.Min(specifiedColumnNames.Count, queryColumns.Count);

            var columnNames = specifiedColumnNames == null
                                  ? queryColumns.Select(c => c.Name)
                                  : specifiedColumnNames.Select(t => t.Identifier.ValueText);

            var columns = queryColumns.Take(columnCount)
                                      .Zip(columnNames, (c, n) => new ColumnSymbol(n, c.Expression.Type))
                                      .Where(c => !string.IsNullOrEmpty(c.Name))
                                      .ToArray();

            var uniqueColumnNames = new HashSet<string>();
            foreach (var column in columns.Where(c => !uniqueColumnNames.Add(c.Name)))
                _diagnostics.ReportCteHasDuplicateColumnName(commonTableExpression.Name, column.Name);

            // Given the bound query and the column list, we can now produce a CTE table symbol.

            var name = commonTableExpression.Name.ValueText;
            var tableSymbol = new CommonTableExpressionSymbol(name, columns.ToArray(), boundQuery);

            return new BoundCommonTableExpression(tableSymbol, boundQuery);
        }

        private BoundCommonTableExpression BindCommonTableExpressionRecursive(CommonTableExpressionSyntax commonTableExpression)
        {
            // Recursive CTEs must have the following structure:
            //
            //    {One or more anchor members}
            //    UNION ALL
            //    {One or more recursive members}

            var rootQuery = commonTableExpression.Query as UnionQuerySyntax;
            if (rootQuery == null || rootQuery.AllKeyword == null)
            {
                _diagnostics.ReportCteDoesNotHaveUnionAll(commonTableExpression.Name);

                if (rootQuery == null)
                    return BindCommonTableExpressionNonRecursive(commonTableExpression);
            }

            var toBeExpanded = new Stack<UnionQuerySyntax>();
            toBeExpanded.Push(rootQuery);

            var anchorMembers = new List<QuerySyntax>();
            var recursiveMembers = new List<QuerySyntax>();

            Action<QuerySyntax> processQuery = q =>
            {
                var qAsUnion = q as UnionQuerySyntax;
                if (qAsUnion != null)
                {
                    toBeExpanded.Push(qAsUnion);
                }
                else if (IsRecursive(commonTableExpression, q))
                {
                    recursiveMembers.Add(q);
                }
                else
                {
                    anchorMembers.Add(q);
                }
            };

            while (toBeExpanded.Count > 0)
            {
                var q = toBeExpanded.Pop();
                processQuery(q.LeftQuery);
                processQuery(q.RightQuery);
            }

            // Ensure we have at least one anchor

            if (anchorMembers.Count == 0)
            {
                _diagnostics.ReportCteDoesNotHaveAnchorMember(commonTableExpression.Name);
                return BindCommonTableExpressionNonRecursive(commonTableExpression);
            }

            BoundQuery boundAnchorQuery = null;

            foreach (var anchorMember in anchorMembers)
            {
                var boundAnchorMember = BindQuery(anchorMember);

                if (boundAnchorQuery == null)
                {
                    boundAnchorQuery = boundAnchorMember;
                }
                else
                {
                    // TODO: Ensure number of columns are identical
                    // TODO: Check that all data types match exactly -- implicit conversions ARE supported here
                    boundAnchorQuery = new BoundCombinedQuery(boundAnchorQuery, BoundQueryCombinator.UnionAll, boundAnchorMember);
                }
            }

            // TODO: We should respect the CTE's column list, if present
            var columns = (boundAnchorQuery == null
                               ? Enumerable.Empty<ColumnSymbol>()
                               : boundAnchorQuery.SelectColumns.Select(c => new ColumnSymbol(c.Name, c.Expression.Type))).ToArray();

            var name = commonTableExpression.Name.ValueText;

            Func<CommonTableExpressionSymbol, IList<BoundQuery>> lazyBoundRecursiveMembers = s =>
            {
                var binder = CreateLocalBinder(s);
                var boundRecursiveMembers = recursiveMembers.Select(binder.BindQuery).ToArray();

                foreach (var boundRecursiveMember in boundRecursiveMembers)
                {
                    // TODO: Check that all column counts match the CTE
                    // TODO: Check that data types of all query columns match exactly the ones from the CTE -- implicit conversion ARE NOT supported here.

                    // TODO: Check conditions below:
                    //if (checker.RecursiveReferenceInSubquery)
                    //    _errorReporter.CteContainsRecursiveReferenceInSubquery(commonTableExpression.TableName);
                    //else if (checker.RecursiveReferences == 0)
                    //    _errorReporter.CteContainsUnexpectedAnchorMember(commonTableExpression.TableName);
                    //else if (checker.RecursiveReferences > 1)
                    //    _errorReporter.CteContainsMultipleRecursiveReferences(commonTableExpression.TableName);

                    //if (checker.ContainsUnion)
                    //    _errorReporter.CteContainsUnion(commonTableExpression.TableName);

                    //if (checker.ContainsDisctinct)
                    //    _errorReporter.CteContainsDistinct(commonTableExpression.TableName);

                    //if (checker.ContainsTop)
                    //    _errorReporter.CteContainsTop(commonTableExpression.TableName);

                    //if (checker.ContainsOuterJoin)
                    //    _errorReporter.CteContainsOuterJoin(commonTableExpression.TableName);

                    //if (checker.ContainsGroupByHavingOrAggregate)
                    //    _errorReporter.CteContainsGroupByHavingOrAggregate(commonTableExpression.TableName);
                }

                return boundRecursiveMembers;
            };

            var commonTableExpressionSymbol = new CommonTableExpressionSymbol(name, columns, boundAnchorQuery, lazyBoundRecursiveMembers);
            return new BoundCommonTableExpression(commonTableExpressionSymbol, boundAnchorQuery);
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node)
        {
            var fromClause = BindFromClause(node.FromClause);

            var binder = fromClause == null
                             ? this
                             : CreateLocalBinder(fromClause.GetDeclaredTableInstances());

            var whereClause = binder.BindWhereClause(node.WhereClause);
            var selectColumns = binder.BindSelectColumns(node.SelectClause.Columns);

            if (node.GroupByClause != null)
            {
                // TODO: Bind GroupByClause            
            }

            var havingClause = binder.BindHavingClause(node.HavingClause);

            // NOTE: We rely on the fact that the parser already ensured the argument to TOP is a valid integer
            //       literal. Thuse, we can simply ignore the case where topClause.Value.Value cannot be casted
            //       to an int -- the parser added the diagnostics already. However, we cannot perform a hard
            //       cast because we also bind input the parser reported errors for.
            var topClause = node.SelectClause.TopClause;
            var top = topClause == null ? null : topClause.Value.Value as int?;
            var withTies = topClause != null && (topClause.TiesKeyword != null || topClause.WithKeyword != null);

            return new BoundSelectQuery(selectColumns, top, withTies, fromClause, whereClause, havingClause);
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

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            return node.TableName != null
                       ? BindWildcardSelectColumnForTable(node.TableName)
                       : BindWildcardSelectColumnForAllTables();
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForTable(SyntaxToken tableName)
        {
            var symbols = LookupTableInstance(tableName).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredTableInstance(tableName);
                return Enumerable.Empty<BoundSelectColumn>();
            }

            if (symbols.Length > 1)
                _diagnostics.ReportAmbiguousName(tableName, symbols);

            var tableInstance = symbols[0];
            return BindWildcardSelectColumnForTableInstance(tableInstance);
        }

        private static IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForTableInstance(TableInstanceSymbol tableInstance)
        {
            return from columnInstance in tableInstance.ColumnInstances
                   let expression = new BoundNameExpression(columnInstance)
                   select new BoundSelectColumn(expression, columnInstance.Name);
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForAllTables()
        {
            var symbols = LookupTableInstances().ToArray();

            return from tableInstance in symbols
                   from column in BindWildcardSelectColumnForTableInstance(tableInstance)
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

            var predicate = BindExpression(node.Predicate);

            if (predicate.Type.IsNonBoolean())
                _diagnostics.ReportWhereClauseMustEvaluateToBool(node.Predicate.Span);

            return predicate;
        }

        private BoundExpression BindHavingClause(HavingClauseSyntax node)
        {
            if (node == null)
                return null;

            var predicate = BindExpression(node.Predicate);

            if (predicate.Type.IsNonBoolean())
                _diagnostics.ReportHavingClauseMustEvaluateToBool(node.Predicate.Span);

            return predicate;
        }
    }
}