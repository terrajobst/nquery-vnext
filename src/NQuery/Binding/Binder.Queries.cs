using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal struct ComputedValue
    {
        private readonly ExpressionSyntax _syntax;
        private readonly BoundExpression _expression;
        private readonly ValueSlot _result;

        public ComputedValue(ExpressionSyntax syntax, BoundExpression expression, ValueSlot result)
        {
            _syntax = syntax;
            _expression = expression;
            _result = result;
        }

        public ExpressionSyntax Syntax
        {
            get { return _syntax; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public ValueSlot Result
        {
            get { return _result; }
        }
    }

    internal sealed class QueryState
    {
        private readonly QueryState _parent;
        private readonly HashSet<TableInstanceSymbol> _introducedTables = new HashSet<TableInstanceSymbol>();
        private readonly List<ComputedValue> _computedGroupings = new List<ComputedValue>();
        private readonly List<ComputedValue> _computedAggregates = new List<ComputedValue>(); 
        private readonly List<ComputedValue> _computedProjections = new List<ComputedValue>(); 
        private readonly Dictionary<ExpressionSyntax, ValueSlot> _replacedExpression = new Dictionary<ExpressionSyntax, ValueSlot>();

        public QueryState(QueryState parent)
        {
            _parent = parent;
        }

        public QueryState Parent
        {
            get { return _parent; }
        }

        public HashSet<TableInstanceSymbol> IntroducedTables
        {
            get { return _introducedTables; }
        }

        public List<ComputedValue> ComputedGroupings
        {
            get { return _computedGroupings; }
        }

        public List<ComputedValue> ComputedAggregates
        {
            get { return _computedAggregates; }
        }

        public List<ComputedValue> ComputedProjections
        {
            get { return _computedProjections; }
        }

        public Dictionary<ExpressionSyntax, ValueSlot> ReplacedExpression
        {
            get { return _replacedExpression; }
        }
    }

    partial class Binder
    {
        public virtual QueryState QueryState
        {
            get { return _parent == null ? null : _parent.QueryState; }
        }

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

        private string InferColumnName(ExpressionSyntax expressionSyntax)
        {
            var nameExpression = _boundNodeFromSynatxNode[expressionSyntax] as BoundNameExpression;
            return nameExpression != null ? nameExpression.Symbol.Name : null;
        }

        private QueryState FindQueryState(TableInstanceSymbol tableInstanceSymbol)
        {
            var queryState = QueryState;
            while (queryState != null)
            {
                if (queryState.IntroducedTables.Contains(tableInstanceSymbol))
                    return queryState;

                queryState = queryState.Parent;
            }

            return null;
        }

        private bool TryReplaceExpression(ExpressionSyntax expression, BoundExpression boundExpression, out ValueSlot valueSlot)
        {
            var queryState = QueryState;

            // If the expression refers to a column we already know the value slot.

            var nameExpression = boundExpression as BoundNameExpression;
            var columnInstance = nameExpression == null ? null : nameExpression.Symbol as ColumnInstanceSymbol;
            if (columnInstance != null)
            {
                valueSlot = columnInstance.ValueSlot;
                queryState.ReplacedExpression.Add(expression, valueSlot);
                return true;
            }

            // Replace existing expression for which we've already allocated
            // a value slot, such as aggregates and groups.

            while (queryState != null)
            {
                var candidates = queryState.ComputedGroupings
                                           .Concat(queryState.ComputedAggregates)
                                           .Concat(queryState.ComputedProjections);
                valueSlot = FindComputedValue(expression, candidates);

                if (valueSlot != null)
                {
                    queryState.ReplacedExpression.Add(expression, valueSlot);
                    return true;
                }

                queryState = queryState.Parent;
            }

            valueSlot = null;
            return false;
        }

        private static bool TryGetExistingValue(BoundExpression boundExpression, out ValueSlot valueSlot)
        {
            var boundValueSlot = boundExpression as BoundValueSlotExpression;
            if (boundValueSlot == null)
            {
                valueSlot = null;
                return false;
            }

            valueSlot = boundValueSlot.ValueSlot;
            return true;
        }

        private static ValueSlot FindComputedValue(ExpressionSyntax expressionSyntax, IEnumerable<ComputedValue> candidates)
        {
            return (from c in candidates
                    where c.Syntax.IsEquivalentTo(expressionSyntax) // TODO: We need to compare symbols as well!
                    select c.Result).FirstOrDefault();
        }

        private void EnsureAllColumnReferencesAreLegal(SelectQuerySyntax node, OrderedQuerySyntax orderedQueryNode)
        {
            var isAggregated = QueryState.ComputedAggregates.Count > 0;
            var isGrouped = QueryState.ComputedGroupings.Count > 0;

            if (!isAggregated && !isGrouped)
                return;

            var selectDiagnosticId = isGrouped
                                         ? DiagnosticId.SelectExpressionNotAggregatedOrGrouped
                                         : DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy;

            EnsureAllColumnReferencesAreAggregatedOrGrouped(node.SelectClause, selectDiagnosticId);

            if (node.HavingClause != null)
                EnsureAllColumnReferencesAreAggregatedOrGrouped(node.HavingClause,
                                                                DiagnosticId.HavingExpressionNotAggregatedOrGrouped);

            if (orderedQueryNode != null)
            {
                var orderByDiagnosticId = isGrouped
                                              ? DiagnosticId.OrderByExpressionNotAggregatedOrGrouped
                                              : DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy;
                foreach (var column in orderedQueryNode.Columns)
                    EnsureAllColumnReferencesAreAggregatedOrGrouped(column, orderByDiagnosticId);
            }
        }

        private void EnsureAllColumnReferencesAreAggregatedOrGrouped(SyntaxNode node, DiagnosticId diagnosticId)
        {
            var invalidColumnReferences = from n in node.DescendantNodes().OfType<ExpressionSyntax>()
                                          where !n.AncestorsAndSelf().OfType<ExpressionSyntax>().Any(IsGroupedOrAggregated)
                                          where _boundNodeFromSynatxNode.ContainsKey(n)
                                          let e = _boundNodeFromSynatxNode[n] as BoundNameExpression
                                          where e != null
                                          let c = e.Symbol as TableColumnInstanceSymbol
                                          where c != null
                                          where QueryState.IntroducedTables.Contains(c.TableInstance)
                                          select n;

            foreach (var invalidColumnReference in invalidColumnReferences)
                _diagnostics.Add(new Diagnostic(invalidColumnReference.Span, diagnosticId, diagnosticId.ToString()));
        }

        private bool IsGroupedOrAggregated(ExpressionSyntax expressionSyntax)
        {
            if (QueryState == null)
                return false;

            ValueSlot valueSlot;
            if (!QueryState.ReplacedExpression.TryGetValue(expressionSyntax, out valueSlot))
                return false;

            var groupsAndAggregates = QueryState.ComputedGroupings.Concat(QueryState.ComputedAggregates);
            return groupsAndAggregates.Select(c => c.Result).Contains(valueSlot);
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
                              .Select(c => new QueryColumnInstanceSymbol(c.Name, _valueSlotFactory.CreateTemporaryValueSlot(c.Type)))
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
                                                  .Select(c => new QueryColumnInstanceSymbol(c.Name, _valueSlotFactory.CreateTemporaryValueSlot(c.Type)))
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
            var queryBinder = CreateQueryBinder();

            var fromClause = queryBinder.BindFromClause(node.FromClause);
            var declaredTableInstances = fromClause == null
                                             ? null
                                             : fromClause.GetDeclaredTableInstances();
            if (declaredTableInstances != null)
                queryBinder.QueryState.IntroducedTables.UnionWith(declaredTableInstances);

            var fromAwareBinder = declaredTableInstances == null
                                      ? queryBinder
                                      : queryBinder.CreateLocalBinder(declaredTableInstances);

            var whereClause = fromAwareBinder.BindWhereClause(node.WhereClause);

            var groupByClause = fromAwareBinder.BindGroupByClause(node.GroupByClause);

            var havingClause = fromAwareBinder.BindHavingClause(node.HavingClause);

            var selectColumns = fromAwareBinder.BindSelectColumns(node.SelectClause.Columns);

            var outputColumns = selectColumns.Select(s => s.Column).ToArray();

            var orderByClause = fromAwareBinder.BindOrderByClause(outputColumns, orderedQueryNode);

            queryBinder.EnsureAllColumnReferencesAreLegal(node, orderedQueryNode);

            var aggregates = (from t in queryBinder.QueryState.ComputedAggregates
                              let aggregate = (BoundAggregateExpression)t.Expression
                              select Tuple.Create(aggregate, t.Result)).ToArray();

            var groups = (from t in queryBinder.QueryState.ComputedGroupings
                          select Tuple.Create(t.Expression, t.Result)).ToArray();

            var projections = (from t in queryBinder.QueryState.ComputedProjections
                               select Tuple.Create(t.Expression, t.Result)).ToArray();

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

            return new BoundSelectQuery(top,
                                        withTies,
                                        fromClause,
                                        whereClause,
                                        aggregates,
                                        groups,
                                        havingClause,
                                        projections,
                                        orderByClause,
                                        outputColumns);
        }

        private IEnumerable<BoundSelectColumn> BindSelectColumns(IEnumerable<SelectColumnSyntax> nodes)
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
            return Bind(node, BindExpressionSelectColumnInternal);
        }

        private BoundSelectColumn BindExpressionSelectColumnInternal(ExpressionSelectColumnSyntax node)
        {
            var expression = node.Expression;
            var boundExpression = BindExpression(expression);
            var name = node.Alias != null
                           ? node.Alias.Identifier.ValueText
                           : InferColumnName(expression);

            ValueSlot valueSlot;
            if (!TryGetExistingValue(boundExpression, out valueSlot))
            {
                valueSlot = _valueSlotFactory.CreateTemporaryValueSlot(boundExpression.Type);
                QueryState.ComputedProjections.Add(new ComputedValue(expression, boundExpression, valueSlot));
            }

            var queryColumn = new QueryColumnInstanceSymbol(name, valueSlot);

            return new BoundSelectColumn(queryColumn);
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
            return selectColumn.QueryColumns.Select(c => new BoundSelectColumn(c));
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

            var boundColumns = new List<ValueSlot>(groupByClause.Columns.Count);

            foreach (var column in groupByClause.Columns)
            {
                // TODO: Ensure datatype that is comparable.
                var expression = column.Expression;
                var boundExpression = groupByBinder.BindExpression(expression);

                ValueSlot valueSlot;
                if (!TryGetExistingValue(boundExpression, out valueSlot))
                    valueSlot = _valueSlotFactory.CreateTemporaryValueSlot(boundExpression.Type);

                // NOTE: Keep this outside the if check because we assume all groups are recorded
                //       -- independent from whether they are based on existing values or not.
                QueryState.ComputedGroupings.Add(new ComputedValue(expression, boundExpression, valueSlot));

                boundColumns.Add(valueSlot);
            }

            return new BoundGroupByClause(boundColumns.ToArray());
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

            // Although ORDER BY can contain abitrary expression, there are special rules to how those
            // expressions relate to the SELECT list of a query:
            //
            // (1) By position
            //     If the selector is a literal integer we'll resolve to a query column by position.
            //
            // (2) By name
            //     If the selector is a NameExpression we'll try to resolve a SELECT column by name.
            //
            // (3) By structure
            //     If the selector matches an expression in the SELECT list, it will bind to it as well.
            //
            // If none of the above is true, ORDER BY may compute a new expression that will be used for
            // ordering the query, but this requires that the query it's applied to is a SELECT query.
            // In other words it can't be a query combined with UNION, EXCEPT, or INTERSECT.

            foreach (var column in node.Columns)
            {
                var selector = column.ColumnSelector;
                var isAscending = column.Modifier == null ||
                                  column.Modifier.Kind == SyntaxKind.AscKeyword;

                ValueSlot valueSlot = null;

                var selectorAsLiteral = selector as LiteralExpressionSyntax;
                var selectorAsName = selector as NameExpressionSyntax;

                // Case (1): Check for positional form.

                if (selectorAsLiteral != null)
                {
                    var position = selectorAsLiteral.Value as int?;
                    if (position != null)
                    {
                        var index = position.Value - 1;
                        var indexValid = 0 <= index && index < queryColumns.Count;
                        if (indexValid)
                        {
                            valueSlot = queryColumns[index].ValueSlot;
                        }
                        else
                        {
                            // Report that the given position isn't valid.
                            _diagnostics.ReportOrderByColumnPositionIsOutOfRange(selector.Span, position.Value, queryColumns.Count);

                            // And to avoid cascading errors, we'll bind to the first column in the query,
                            // which is guaranteed to exist.
                            valueSlot = queryColumns[0].ValueSlot;
                        }
                    }
                }

                // Case (2): Check for query column name.

                else if (selectorAsName != null)
                {
                    var columnSymbols = selectorBinder.LookupQueryColumn(selectorAsName.Name).ToArray();
                    if (columnSymbols.Length > 0)
                    {
                        if (columnSymbols.Length > 1)
                            _diagnostics.ReportAmbiguousColumnInstance(selectorAsName.Name, columnSymbols);

                        var columnSymbol = columnSymbols[0];

                        // Since this name isn't bound as a regular expression we simple fake this one up.
                        // This ensures that this name appears to be bound like any other expression.
                        Bind(selectorAsName, new BoundNameExpression(columnSymbol));

                        valueSlot = columnSymbol.ValueSlot;
                    }
                }

                // Case (3): Bind regular expression.

                if (valueSlot == null)
                {
                    var boundSelector = selectorBinder.BindExpression(selector);

                    if (boundSelector is BoundLiteralExpression)
                        _diagnostics.ReportConstantExpressionInOrderBy(selector.Span);

                    if (!TryGetExistingValue(boundSelector, out valueSlot))
                    {
                        // TODO: Ensure we are applied to a SELECT query.
                        valueSlot = _valueSlotFactory.CreateTemporaryValueSlot(boundSelector.Type);
                        QueryState.ComputedProjections.Add(new ComputedValue(selector, boundSelector, valueSlot));
                    }
                }

                var queryColumn = queryColumns.SingleOrDefault(c => c.ValueSlot == valueSlot);
                var boundColumn = new BoundOrderByColumn(queryColumn, valueSlot, isAscending);
                Bind(column, boundColumn);
                boundColumns.Add(boundColumn);
            }

            return new BoundOrderByClause(boundColumns);
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
}