using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    partial class Binder
    {
        private static SelectQuerySyntax GetAppliedSelectQuery(QuerySyntax node)
        {
            while (node is ParenthesizedQuerySyntax)
            {
                var parenthesizedQuery = (ParenthesizedQuerySyntax)node;
                node = parenthesizedQuery.Query;
            }

            return node as SelectQuerySyntax;
        }

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

        private BoundQuery BindSubquery(QuerySyntax querySyntax)
        {
            if (InGroupByClause)
            {
                Diagnostics.ReportGroupByCannotContainSubquery(querySyntax.Span);
            }
            else if (InAggregateArgument)
            {
                Diagnostics.ReportAggregateCannotContainSubquery(querySyntax.Span);
            }

            return BindQuery(querySyntax);
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
            // TODO: Ensure all column data types are comparable.

            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var columns = left.OutputColumns;

            return new BoundCombinedQuery(left, BoundQueryCombinator.Except, right, columns);
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            // TODO: Check column count
            // TODO: Ensure all types are identical, if not we need to insert conversion operators
            // TODO: If ALL is not specified, ensure all column data types are comparable.

            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var combinator = node.AllKeyword == null
                                 ? BoundQueryCombinator.Union
                                 : BoundQueryCombinator.UnionAll;
            var columns = left.OutputColumns
                              .Select(c => new QueryColumnInstanceSymbol(c.Name, c.Syntax, _valueSlotFactory.CreateTemporaryValueSlot(c.Type)))
                              .ToArray();

            return new BoundCombinedQuery(left, combinator, right, columns);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            // TODO: Check column count
            // TODO: Ensure all types are identical, if not we need to insert conversion operators
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var columns = left.OutputColumns;
            return new BoundCombinedQuery(left, BoundQueryCombinator.Intersect, right, columns);
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            var selectQuery = GetAppliedSelectQuery(node.Query);
            if (selectQuery != null)
                return BindSelectQuery(selectQuery, node);

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

            // TODO: Ensure that all ORDER BY datatypes are comparable.
            // TODO: Ensure that no constant expression is in ORDER BY
            // TODO: Ensure that all ORDER BY expressions are present in the input.

            var query = BindQuery(node.Query);
            var orderByClause = BindOrderByClause(query.OutputColumns, node);

            return new BoundOrderedQuery(query, orderByClause.Columns);
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

            var queryColumns = boundQuery.OutputColumns;

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
                                      .Zip(columnNames, (c, n) => new ColumnSymbol(n, c.Type))
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
                    var outputColumns = boundAnchorQuery.OutputColumns
                                                  .Select(c => new QueryColumnInstanceSymbol(c.Name, c.Syntax, _valueSlotFactory.CreateTemporaryValueSlot(c.Type)))
                                                  .ToArray();

                    boundAnchorQuery = new BoundCombinedQuery(boundAnchorQuery, BoundQueryCombinator.UnionAll, boundAnchorMember, outputColumns);
                }
            }

            // TODO: We should respect the CTE's column list, if present
            var columns = (boundAnchorQuery == null
                               ? Enumerable.Empty<ColumnSymbol>()
                               : boundAnchorQuery.OutputColumns.Select(c => new ColumnSymbol(c.Name, c.Type))).ToArray();

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
            return BindSelectQuery(node, null);
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node, OrderedQuerySyntax orderedQueryNode)
        {
            var fromClause = BindFromClause(node.FromClause);

            var queryBinder = CreateQueryBinder(fromClause);

            var whereClause = queryBinder.BindWhereClause(node.WhereClause);

            var groupByClause = queryBinder.BindGroupByClause(node.GroupByClause);

            var havingClause = queryBinder.BindHavingClause(node.HavingClause);

            // TODO: The aggregations should be filtered to those which only contain columns that are part of this query.
            // TODO: Check that no aggregations mix columns from different queries
            // TODO: Aggregates that have same combination of function/argument shouldn't be computed twice.

            var selectColumns = queryBinder.BindSelectColumns(node.SelectClause.Columns);

            var outputColumns = selectColumns
                                  .Select(s => new QueryColumnInstanceSymbol(s.Name, s.Syntax, s.ValueSlot))
                                  .ToArray();

            var orderByClause = queryBinder.BindOrderByClause(outputColumns, orderedQueryNode);

            var localValues = queryBinder.ExpressionValueSlotFactory.LocalValues
                             .Select(t => new BoundProjectedValue(t.Item1, t.Item2))
                             .ToArray();

            // TODO: If GROUP BY is specified, ensure the following conditions:
            //
            //        1. All expressions in GROUP BY must have a datatype that is comparable.
            //        2. All expressions in GROUP BY must not be aggregated.
            //        3. All expressions in GROUP BY must not contain subqueries.
            //        3. All expressions in SELECT, ORDER BY, and HAVING must be aggregated,
            //           grouped or must not reference columns.
            //
            // TODO: If aggregation is required by no GROUP BY is specified, ensure the following:
            //
            //        All expressions in SELECT, ORDER BY, and HAVING are either aggregated or
            //        do not reference any column.

            // TODO: If DISTINCT is specified, ensure that all column sources are datatypes that are comparable.
            // TODO: If DISTINCT is specified, ensure that all ORDER BY expressions are contained in SELECT

            // NOTE: We rely on the fact that the parser already ensured the argument to TOP is a valid integer
            //       literal. Thuse, we can simply ignore the case where topClause.Value.Value cannot be casted
            //       to an int -- the parser added the diagnostics already. However, we cannot perform a hard
            //       cast because we also bind input the parser reported errors for.
            var topClause = node.SelectClause.TopClause;
            var top = topClause == null ? null : topClause.Value.Value as int?;
            var withTies = topClause != null && (topClause.TiesKeyword != null || topClause.WithKeyword != null);

            if (withTies && orderedQueryNode == null)
            {
                // TODO: ERROR - we require an ORDER BY
            }

            return new BoundSelectQuery(top, withTies, selectColumns, fromClause, whereClause, localValues, groupByClause, havingClause, orderByClause, outputColumns);
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
                        var wildcardSelectColumn = BindWildcardSelectColumn((WildcardSelectColumnSyntax)node);
                        var boundColumns = BindSelectColumns(wildcardSelectColumn);
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
            var expression = node.Expression;
            var boundExpression = BindExpression(expression);
            var name = node.Alias != null
                           ? node.Alias.Identifier.ValueText
                           : InferColumnName(boundExpression);

            var valueSlot = ExpressionValueSlotFactory.GetOrCreateValueSlot(expression, boundExpression);

            return new BoundSelectColumn(name, expression, valueSlot);
        }

        private BoundWildcardSelectColumn BindWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            return Bind(node, BindWildcardSelectColumnInternal);
        }

        private BoundWildcardSelectColumn BindWildcardSelectColumnInternal(WildcardSelectColumnSyntax node)
        {
            return node.TableName != null
                       ? BindWildcardSelectColumnForTable(node.TableName)
                       : BindWildcardSelectColumnForAllTables(node.AsteriskToken);
        }

        private BoundWildcardSelectColumn BindWildcardSelectColumnForTable(SyntaxToken tableName)
        {
            var symbols = LookupTableInstance(tableName).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredTableInstance(tableName);
                return new BoundWildcardSelectColumn(null, new TableColumnInstanceSymbol[0]);
            }

            if (symbols.Length > 1)
                _diagnostics.ReportAmbiguousName(tableName, symbols);

            var tableInstance = symbols[0];
            var columnInstances = tableInstance.ColumnInstances;
            return new BoundWildcardSelectColumn(tableInstance, columnInstances);
        }

        private BoundWildcardSelectColumn BindWildcardSelectColumnForAllTables(SyntaxToken asteriskToken)
        {
            var tableInstances = LookupTableInstances().ToArray();

            if (tableInstances.Length == 0)
            {
                // Normally, SELECT * is disallowed.
                //
                // But if the current query is contained in an EXISTS query, that's considered OK.
                // Please note that finding our parent doesn't require handling query combinators,
                // such as UNION/EXCEPT/INTERSECT, as those are invalid. Same is true for ordered
                // queries.
                //
                // We want, however, skip any parenthesized queries in the process.

                var query = asteriskToken.Parent.AncestorsAndSelf()
                                         .OfType<SelectQuerySyntax>()
                                         .First();

                var firstRealParent = query.Parent.AncestorsAndSelf()
                                           .SkipWhile(n => n is ParenthesizedQuerySyntax)
                                           .FirstOrDefault();

                var isInExists = firstRealParent is ExistsSubselectSyntax;
                if (!isInExists)
                    _diagnostics.ReportMustSpecifyTableToSelectFrom(asteriskToken.Span);
            }

            var columnInstances = tableInstances.SelectMany(t => t.ColumnInstances).ToArray();
            return new BoundWildcardSelectColumn(null, columnInstances);
        }

        private static IEnumerable<BoundSelectColumn> BindSelectColumns(BoundWildcardSelectColumn selectColumn)
        {
            return from columnInstance in selectColumn.Columns
                   select new BoundSelectColumn(columnInstance.Name, null, columnInstance.ValueSlot);
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
                    lastTableReference = new BoundJoinedTableReference(BoundJoinType.Inner, lastTableReference, boundTableReference, null);
                }
            }

            return lastTableReference;
        }

        private BoundExpression BindWhereClause(WhereClauseSyntax node)
        {
            if (node == null)
                return null;

            var binder = CreateWhereClauseBinder();
            var predicate = binder.BindExpression(node.Predicate);

            if (predicate.Type.IsNonBoolean())
                _diagnostics.ReportWhereClauseMustEvaluateToBool(node.Predicate.Span);

            return predicate;
        }

        private BoundGroupByClause BindGroupByClause(GroupByClauseSyntax groupByClause)
        {
            if (groupByClause == null)
                return null;

            var groupByBinder = CreateGroupByClauseBinder();

            var boundColumns = (from column in groupByClause.Columns
                                let expression = column.Expression
                                let boundExpression = groupByBinder.BindExpression(expression)
                                select ExpressionValueSlotFactory.GetOrCreateValueSlot(expression, boundExpression)).ToArray();

            return new BoundGroupByClause(boundColumns);
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

        private BoundOrderByClause BindOrderByClause(IList<QueryColumnInstanceSymbol> queryColumns, OrderedQuerySyntax node)
        {
            if (node == null)
                return null;

            var selectorBinder = CreateLocalBinder(queryColumns);

            var boundColumns = new List<BoundOrderByColumn>();

            foreach (var column in node.Columns)
            {
                var selector = column.ColumnSelector;
                var isAscending = column.Modifier == null ||
                                  column.Modifier.Kind == SyntaxKind.AscKeyword;
                var boundSelector = selectorBinder.BindExpression(selector);
                var boundLiteral = boundSelector as BoundLiteralExpression;

                ValueSlot slot;

                if (boundLiteral != null && boundLiteral.Type == typeof (int))
                {
                    var index = ((int) boundLiteral.Value) - 1;
                    // TODO: Ensure index is valid
                    slot = queryColumns[index].ValueSlot;
                }
                else
                {
                    // TODO: Ensure boundSelector isn't a constant expression.
                    slot = selectorBinder.ExpressionValueSlotFactory.GetOrCreateValueSlot(selector, boundSelector);
                }

                var boundColumn = new BoundOrderByColumn(slot, isAscending);
                boundColumns.Add(boundColumn);
            }

            return new BoundOrderByClause(boundColumns);
        }
    }

    internal sealed class BoundProjectedValue
    {
        private readonly ValueSlot _valueSlot;
        private readonly BoundExpression _expression;

        public BoundProjectedValue(ValueSlot valueSlot, BoundExpression expression)
        {
            _valueSlot = valueSlot;
            _expression = expression;
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }
    }

    internal sealed class BoundGroupByClause
    {
        private readonly ReadOnlyCollection<ValueSlot> _columns;

        public BoundGroupByClause(IList<ValueSlot> columns)
        {
            _columns = new ReadOnlyCollection<ValueSlot>(columns);
        }

        public ReadOnlyCollection<ValueSlot> Columns
        {
            get { return _columns; }
        }
    }

    internal sealed class BoundOrderByClause
    {
        private readonly ReadOnlyCollection<BoundOrderByColumn> _columns;

        public BoundOrderByClause(IList<BoundOrderByColumn> columns)
        {
            _columns = new ReadOnlyCollection<BoundOrderByColumn>(columns);
        }

        public ReadOnlyCollection<BoundOrderByColumn> Columns
        {
            get { return _columns; }
        }
    }

    internal sealed class BoundValueSlotExpression : BoundExpression
    {
        private readonly ValueSlot _valueSlot;

        public BoundValueSlotExpression(ValueSlot valueSlot)
        {
            _valueSlot = valueSlot;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ValueSlotExpression; }
        }

        public override Type Type
        {
            get { return _valueSlot.Type; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }
    }

    internal sealed class BoundOrderByColumn
    {
        private readonly ValueSlot _valueSlot;
        private readonly bool _isAscending;

        public BoundOrderByColumn(ValueSlot valueSlot, bool isAscending)
        {
            _valueSlot = valueSlot;
            _isAscending = isAscending;
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public bool IsAscending
        {
            get { return _isAscending; }
        }
    }

    internal sealed class BoundOrderedQuery : BoundQuery
    {
        private readonly BoundQuery _input;
        private readonly IList<BoundOrderByColumn> _columns;

        public BoundOrderedQuery(BoundQuery input, IList<BoundOrderByColumn> columns)
        {
            _input = input;
            _columns = columns;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderedQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _input.OutputColumns; }
        }

        public BoundQuery Input
        {
            get { return _input; }
        }

        public IList<BoundOrderByColumn> Columns
        {
            get { return _columns; }
        }
    }

    internal sealed class BoundTopQuery : BoundQuery
    {
        private readonly BoundQuery _input;
        private readonly int _limit;
        private readonly bool _withTies;

        public BoundTopQuery(BoundQuery input, int limit, bool withTies)
        {
            _input = input;
            _limit = limit;
            _withTies = withTies;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TopQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _input.OutputColumns; }
        }

        public BoundQuery Input
        {
            get { return _input; }
        }

        public int Limit
        {
            get { return _limit; }
        }

        public bool WithTies
        {
            get { return _withTies; }
        }
    }

}