using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NQuery.Iterators;
using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Binding
{
    partial class Binder
    {
        public virtual BoundQueryState QueryState
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

        private static SelectQuerySyntax GetFirstSelectQuery(QuerySyntax node)
        {
            var p = node as ParenthesizedQuerySyntax;
            if (p != null)
                return GetFirstSelectQuery(p.Query);

            var u = node as UnionQuerySyntax;
            if (u != null)
                return GetFirstSelectQuery(u.LeftQuery);

            var e = node as ExceptQuerySyntax;
            if (e != null)
                return GetFirstSelectQuery(e.LeftQuery);

            var i = node as IntersectQuerySyntax;
            if (i != null)
                return GetFirstSelectQuery(i.LeftQuery);

            var o = node as OrderedQuerySyntax;
            if (o != null)
                return GetFirstSelectQuery(o.Query);

            return (SelectQuerySyntax) node;
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
            var columnExpression = GetBoundNode<BoundColumnExpression>(expressionSyntax);
            return columnExpression != null ? columnExpression.Symbol.Name : null;
        }

        private BoundQueryState FindQueryState(TableInstanceSymbol tableInstanceSymbol)
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

            var columnExpression = boundExpression as BoundColumnExpression;
            var columnInstance = columnExpression == null ? null : columnExpression.Symbol;
            if (columnInstance != null && queryState != null)
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

        private static ValueSlot FindComputedValue(ExpressionSyntax expressionSyntax, IEnumerable<BoundComputedValueWithSyntax> candidates)
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
            var wildcardReferences = from n in node.DescendantNodes().OfType<WildcardSelectColumnSyntax>()
                                     let bw = GetBoundNode<BoundWildcardSelectColumn>(n)
                                     where bw != null
                                     from c in bw.TableColumns
                                     where !IsGroupedOrAggregated(c.ValueSlot)
                                     select Tuple.Create((SyntaxNode)n, c);

            var expressionReferences = from n in node.DescendantNodes().OfType<ExpressionSyntax>()
                                       where !n.AncestorsAndSelf().OfType<ExpressionSyntax>().Any(IsGroupedOrAggregated)
                                       let e = GetBoundNode<BoundColumnExpression>(n)
                                       where e != null
                                       let c = e.Symbol as TableColumnInstanceSymbol
                                       where c != null
                                       select Tuple.Create((SyntaxNode) n, c);

            var invalidColumnReferences = from t in wildcardReferences.Concat(expressionReferences)
                                          where QueryState.IntroducedTables.Contains(t.Item2.TableInstance)
                                          select t;

            foreach (var invalidColumnReference in invalidColumnReferences)
            {
                var symbolSpan = invalidColumnReference.Item1.Span;
                var symbol = invalidColumnReference.Item2;
                Diagnostics.Report(symbolSpan, diagnosticId, symbol.Name);
            }
        }

        private bool IsGroupedOrAggregated(ExpressionSyntax expressionSyntax)
        {
            if (QueryState == null)
                return false;

            ValueSlot valueSlot;
            if (!QueryState.ReplacedExpression.TryGetValue(expressionSyntax, out valueSlot))
                return false;

            return IsGroupedOrAggregated(valueSlot);
        }

        private bool IsGroupedOrAggregated(ValueSlot valueSlot)
        {
            var groupsAndAggregates = QueryState.ComputedGroupings.Concat(QueryState.ComputedAggregates);
            return groupsAndAggregates.Select(c => c.Result).Contains(valueSlot);
        }

        private IComparer BindComparer(Type type, TextSpan errorSpan, DiagnosticId errorId)
        {
            var missingComparer = Comparer.Default;

            if (type.IsError())
                return missingComparer;

            var comparer = LookupComparer(type);
            if (comparer == null)
            {
                comparer = missingComparer;
                Diagnostics.Report(errorSpan, errorId, type.ToDisplayName());
            }

            return comparer;
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
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var columns = left.OutputColumns;

            if (left.OutputColumns.Count != right.OutputColumns.Count)
                Diagnostics.ReportDifferentExpressionCountInBinaryQuery(node.ExceptKeyword.Span);

            BoundRelation leftInput;
            BoundRelation rightInput;
            var leftOutputValues = BindToCommonTypes(node.ExceptKeyword.Span, left, right, out leftInput, out rightInput);

            var outputValues = leftOutputValues;
            var outputColumns = BindOutputColumns(columns, outputValues);

            foreach (var column in outputColumns)
                BindComparer(column.Type, node.ExceptKeyword.Span, DiagnosticId.InvalidDataTypeInExcept);

            var relation = new BoundCombinedRelation(BoundQueryCombinator.Except, leftInput, rightInput, outputValues);
            return new BoundQuery(relation, outputColumns);
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var combinator = node.AllKeyword == null
                                 ? BoundQueryCombinator.Union
                                 : BoundQueryCombinator.UnionAll;

            if (left.OutputColumns.Count != right.OutputColumns.Count)
                Diagnostics.ReportDifferentExpressionCountInBinaryQuery(node.UnionKeyword.Span);

            BoundRelation leftInput;
            BoundRelation rightInput;
            var leftOutputValues = BindToCommonTypes(node.UnionKeyword.Span, left, right, out leftInput, out rightInput);

            var outputValues = leftOutputValues.Select(c => ValueSlotFactory.CreateTemporaryValueSlot(c.Type)).ToArray();
            var outputColumns = BindOutputColumns(left.OutputColumns, outputValues);

            if (combinator == BoundQueryCombinator.Union)
            {
                foreach (var column in outputColumns)
                    BindComparer(column.Type, node.UnionKeyword.Span, DiagnosticId.InvalidDataTypeInUnion);
            }

            var relation = new BoundCombinedRelation(combinator, leftInput, rightInput, outputValues);
            return new BoundQuery(relation, outputColumns);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var columns = left.OutputColumns;

            if (left.OutputColumns.Count != right.OutputColumns.Count)
                Diagnostics.ReportDifferentExpressionCountInBinaryQuery(node.IntersectKeyword.Span);

            BoundRelation leftInput;
            BoundRelation rightInput;
            var leftOutputValues = BindToCommonTypes(node.IntersectKeyword.Span, left, right, out leftInput, out rightInput);

            var outputValues = leftOutputValues;
            var outputColumns = BindOutputColumns(columns, outputValues);

            foreach (var column in outputColumns)
                BindComparer(column.Type, node.IntersectKeyword.Span, DiagnosticId.InvalidDataTypeInIntersect);

            var relation = new BoundCombinedRelation(BoundQueryCombinator.Intersect, leftInput, rightInput, outputValues);
            return new BoundQuery(relation, outputColumns);
        }

        private IList<ValueSlot> BindToCommonTypes(TextSpan errorSpan, BoundQuery left, BoundQuery right, out BoundRelation newLeft, out BoundRelation newRight)
        {
            var columnCount = Math.Min(left.OutputColumns.Count, right.OutputColumns.Count);

            var leftComputedValues = new List<BoundComputedValue>(columnCount);
            var leftOutputs = new List<ValueSlot>(columnCount);

            var rightComputedValues = new List<BoundComputedValue>(columnCount);
            var rightOutputs = new List<ValueSlot>(columnCount);

            for (var i = 0; i < columnCount; i++)
            {
                var leftValue = left.OutputColumns[i].ValueSlot;
                var rightValue = right.OutputColumns[i].ValueSlot;

                BoundExpression convertedLeft;
                BoundExpression convertedRight;
                BindToCommonType(errorSpan, leftValue, rightValue, out convertedLeft, out convertedRight);

                if (convertedLeft == null)
                {
                    leftOutputs.Add(leftValue);
                }
                else
                {
                    var newLeftValue = ValueSlotFactory.CreateTemporaryValueSlot(convertedLeft.Type);
                    var computedValue = new BoundComputedValue(convertedLeft, newLeftValue);
                    leftComputedValues.Add(computedValue);
                    leftOutputs.Add(newLeftValue);
                }

                if (convertedRight == null)
                {
                    rightOutputs.Add(rightValue);
                }
                else
                {
                    var newRightValue = ValueSlotFactory.CreateTemporaryValueSlot(convertedRight.Type);
                    var computedValue = new BoundComputedValue(convertedRight, newRightValue);
                    rightComputedValues.Add(computedValue);
                    rightOutputs.Add(newRightValue);
                }
            }

            newLeft = leftComputedValues.Count == 0
                ? left.Relation
                : new BoundProjectRelation(new BoundComputeRelation(left.Relation, leftComputedValues), leftOutputs);

            newRight = rightComputedValues.Count == 0
                ? right.Relation
                : new BoundProjectRelation(new BoundComputeRelation(right.Relation, rightComputedValues), rightOutputs);

            return leftOutputs;
        }

        private static IList<QueryColumnInstanceSymbol> BindOutputColumns(IList<QueryColumnInstanceSymbol> inputColumns, IList<ValueSlot> outputValues)
        {
            var result = new List<QueryColumnInstanceSymbol>(inputColumns.Count);

            for (var i = 0; i < inputColumns.Count; i++)
            {
                if (i >= outputValues.Count)
                    break;

                var queryColum = inputColumns[i];
                var valueSlot = outputValues[i];

                var resultColumn = queryColum.ValueSlot == valueSlot
                    ? queryColum
                    : new QueryColumnInstanceSymbol(queryColum.Name, valueSlot);

                result.Add(resultColumn);
            }

            return result;
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            // Depending on the query the ORDER BY was applied on, the binding rules
            // differ.
            //
            // (1) If the query is applied to a SELECT query, then ORDER BY may have
            //     more expressions then the underlying SELECT list.
            //
            // (2) If the query is applied to a query combined with UNION, INTERSECT
            //     or EXCEPT all expressions must already be present in the underlying
            //     select list.
            //
            // We implement both cases differently. The first case is actually handled
            // directly when binding the SELECT query because the underlying query has
            // to differentiate between computed columns and output columns.
            //
            // However, both cases eventually call into BindOrderByClause.

            var selectQuery = GetAppliedSelectQuery(node.Query);
            if (selectQuery != null)
            {
                // This is case (1). We bind the select query and pass in ourselves.
                return BindSelectQuery(selectQuery, node);
            }

            // Alright, this is case (2) where we're applied to some sort of combined
            // query.
            // 
            // We will first bind the query in the regular way and then retrieve the
            // bound node for the first query. That node has all the information we
            // need, in particular the binder that has the table context we need to
            // bind the ORDER BY columns.

            var query = BindQuery(node.Query);
            var firstQuerySyntax = GetFirstSelectQuery(node);
            var firstQuery = GetBoundNode<BoundQuery>(firstQuerySyntax);
            var firstFromClauseSyntax = firstQuerySyntax.FromClause;
            var firstFromClause = firstFromClauseSyntax == null
                                    ? null
                                    : GetBoundNode<BoundRelation>(firstFromClauseSyntax);

            var binder = firstFromClause == null
                            ? this
                            : _sharedBinderState.BinderFromBoundNode[firstFromClause];

            // Now, when we bind the ORDER BY clause we have to bind the expressions in
            // in the context of the first query. This also means that all value slots
            // will be local to that query. However, the bound ORDER BY we want to return
            // here has to use the value slots that correspond to our input query.
            // The correspondence is based on their position. Fortunately, BindOrderByClause
            // will perform that mapping for us, but we have to pass in the value slots
            // of the first query and the query columns of our input.

            var inputQueryColumns = firstQuery.OutputColumns;
            var outputQueryCoumns = query.OutputColumns;
            var orderByClause = binder.BindOrderByClause(node, inputQueryColumns, outputQueryCoumns);

            var relation = new BoundSortRelation(query.Relation, orderByClause.Columns.Select(c => c.SortedValue).ToArray());
            return new BoundQuery(relation, outputQueryCoumns);
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
                    Diagnostics.ReportCteHasDuplicateTableName(commonTableExpression.Name);

                currentBinder = currentBinder.CreateLocalBinder(tableSymbol);

            }

            return currentBinder.BindQuery(node.Query);
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
                        Diagnostics.ReportNoColumnAliasSpecified(commonTableExpression.Name, i);
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
                    Diagnostics.ReportCteHasMoreColumnsThanSpecified(commonTableExpression.Name);
                }
                else if (actualCount < specifiedCount)
                {
                    Diagnostics.ReportCteHasFewerColumnsThanSpecified(commonTableExpression.Name);
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
                Diagnostics.ReportCteHasDuplicateColumnName(commonTableExpression.Name, column.Name);

            // Given the bound query and the column list, we can now produce a CTE table symbol.

            var name = commonTableExpression.Name.ValueText;
            var tableSymbol = new CommonTableExpressionSymbol(name, columns.ToArray(), boundQuery);

            return new BoundCommonTableExpression(tableSymbol);
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
                Diagnostics.ReportCteDoesNotHaveUnionAll(commonTableExpression.Name);

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
                Diagnostics.ReportCteDoesNotHaveAnchorMember(commonTableExpression.Name);
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
                                                  .Select(c => new QueryColumnInstanceSymbol(c.Name, ValueSlotFactory.CreateTemporaryValueSlot(c.Type)))
                                                  .ToArray();

                    var combinedRelation = new BoundCombinedRelation(BoundQueryCombinator.UnionAll, boundAnchorQuery.Relation, boundAnchorMember.Relation, outputColumns.Select(c => c.ValueSlot).ToArray());
                    boundAnchorQuery = new BoundQuery(combinedRelation, outputColumns);
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
            return new BoundCommonTableExpression(commonTableExpressionSymbol);
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

            var fromAwareBinder = fromClause == null
                                      ? queryBinder
                                      : _sharedBinderState.BinderFromBoundNode[fromClause];

            var whereClause = fromAwareBinder.BindWhereClause(node.WhereClause);

            var groupByClause = fromAwareBinder.BindGroupByClause(node.GroupByClause);

            var havingClause = fromAwareBinder.BindHavingClause(node.HavingClause);

            var selectColumns = fromAwareBinder.BindSelectColumns(node.SelectClause.Columns);

            var outputColumns = selectColumns.Select(s => s.Column).ToArray();

            var orderByClause = fromAwareBinder.BindOrderByClause(orderedQueryNode, outputColumns, outputColumns);

            queryBinder.EnsureAllColumnReferencesAreLegal(node, orderedQueryNode);

            var aggregates = (from t in queryBinder.QueryState.ComputedAggregates
                              let expression = (BoundAggregateExpression)t.Expression
                              select new BoundAggregatedValue(t.Result, expression.Aggregate, expression.Argument)).ToArray();

            var groups = (from t in queryBinder.QueryState.ComputedGroupings
                          select new BoundComputedValue(t.Expression, t.Result)).ToArray();

            var projections = (from t in queryBinder.QueryState.ComputedProjections
                               select new BoundComputedValue(t.Expression, t.Result)).ToArray();

            var distinctKeyword = node.SelectClause.DistinctAllKeyword;
            var isDistinct = distinctKeyword != null &&
                             distinctKeyword.Kind == SyntaxKind.DistinctKeyword;

            if (isDistinct)
            {
                foreach (var column in outputColumns)
                    BindComparer(column.Type, distinctKeyword.Span, DiagnosticId.InvalidDataTypeInSelectDistinct);
            }

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

            // Putting it together

            var fromRelation = fromClause ?? new BoundConstantRelation();

            var whereRelation = whereClause == null
                ? fromRelation
                : new BoundFilterRelation(fromRelation, whereClause);

            var groupComputeRelation = !groups.Any()
                                            ? whereRelation
                                            : new BoundComputeRelation(whereRelation, groups);

            var groupByAndAggregationRelation = !groups.Any() && !aggregates.Any()
                ? groupComputeRelation
                : new BoundGroupByAndAggregationRelation(groupComputeRelation, groups.Select(g => g.ValueSlot).ToArray(), aggregates);

            var havingRelation = havingClause == null
                ? groupByAndAggregationRelation
                : new BoundFilterRelation(groupByAndAggregationRelation, havingClause);

            var selectComputeRelation = projections.Length == 0
                ? havingRelation
                : new BoundComputeRelation(havingRelation, projections);

            var sortedValues = orderByClause == null
                ? null
                : orderByClause.Columns.Select(c => c.SortedValue).ToArray();

            var sortRelation = sortedValues == null
                ? selectComputeRelation
                : new BoundSortRelation(selectComputeRelation, sortedValues);

            var tieEntries = top == null || sortedValues == null || !withTies
                ? new BoundSortedValue[0]
                : sortedValues;

            var topRelation = top == null
                ? sortRelation
                : new BoundTopRelation(sortRelation, top.Value, tieEntries);

            var projectRelation = new BoundProjectRelation(topRelation, outputColumns.Select(c => c.ValueSlot).ToArray());

            return new BoundQuery(projectRelation, outputColumns);
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
                valueSlot = ValueSlotFactory.CreateTemporaryValueSlot(boundExpression.Type);
                QueryState.ComputedProjections.Add(new BoundComputedValueWithSyntax(expression, boundExpression, valueSlot));
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
                Diagnostics.ReportUndeclaredTableInstance(tableName);
                return new BoundWildcardSelectColumn(null, new TableColumnInstanceSymbol[0]);
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousName(tableName, symbols);

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
                    Diagnostics.ReportMustSpecifyTableToSelectFrom(asteriskToken.Span);
            }

            var columnInstances = tableInstances.SelectMany(t => t.ColumnInstances).ToArray();
            return new BoundWildcardSelectColumn(null, columnInstances);
        }

        private static IEnumerable<BoundSelectColumn> BindSelectColumns(BoundWildcardSelectColumn selectColumn)
        {
            return selectColumn.QueryColumns.Select(c => new BoundSelectColumn(c));
        }

        private BoundRelation BindFromClause(FromClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundNode = BindFromClauseInternal(node);
            var binder = CreateLocalBinder(boundNode.GetDeclaredTableInstances());

            _sharedBinderState.BoundNodeFromSynatxNode.Add(node, boundNode);
            _sharedBinderState.BinderFromBoundNode[boundNode] = binder;

            return boundNode;
        }

        private BoundRelation BindFromClauseInternal(FromClauseSyntax node)
        {
            BoundRelation lastTableReference = null;

            foreach (var tableReference in node.TableReferences)
            {
                var boundTableReference = BindTableReference(tableReference);

                if (lastTableReference == null)
                {
                    lastTableReference = boundTableReference;
                }
                else
                {
                    lastTableReference = new BoundJoinRelation(BoundJoinType.Inner, lastTableReference, boundTableReference, null);
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
                Diagnostics.ReportWhereClauseMustEvaluateToBool(node.Predicate.Span);

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
                var expression = column.Expression;
                var boundExpression = groupByBinder.BindExpression(expression);
                var expressionType = boundExpression.Type;

                // TODO: We need to very expression references at least one column that is not an outer reference.

                BindComparer(expressionType, expression.Span, DiagnosticId.InvalidDataTypeInGroupBy);

                ValueSlot valueSlot;
                if (!TryGetExistingValue(boundExpression, out valueSlot))
                    valueSlot = ValueSlotFactory.CreateTemporaryValueSlot(expressionType);

                // NOTE: Keep this outside the if check because we assume all groups are recorded
                //       -- independent from whether they are based on existing values or not.
                QueryState.ComputedGroupings.Add(new BoundComputedValueWithSyntax(expression, boundExpression, valueSlot));

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
                Diagnostics.ReportHavingClauseMustEvaluateToBool(node.Predicate.Span);

            return predicate;
        }

        private BoundOrderByClause BindOrderByClause(OrderedQuerySyntax node, IList<QueryColumnInstanceSymbol> selectorQueryColumns, IList<QueryColumnInstanceSymbol> resultQueryColumns)
        {
            if (node == null)
                return null;

            // We are called from two places:
            //
            // (1) An ORDER BY applied to a SELECT query
            // (2) An ORDER BY applied to a combined query
            //
            // In the first case, selectorQueryColumns and resultQueryColumns are the same.
            // In the second case, they are different. The selectorQueryColumns represent
            // the output columns of the first SELECT query the ORDER BY is applied to.
            // The resultQueryColumns represent the output columns of the query the ORDER BY
            // is actually applied to (such as a UNION query).
            //
            // We want to return a bound ORDER BY which has the columns bound against the
            // actual query.In order map from the selectors to the result query columns
            // we will use their ordinals.

            var selectorsMustBeInInput = !ReferenceEquals(selectorQueryColumns, resultQueryColumns);
            var getOrdinalFromSelectorValueSlot = selectorQueryColumns.Select((c, i) => Tuple.Create(c.ValueSlot, i))
                                                                      .GroupBy(t => t.Item1, t => t.Item2)
                                                                      .ToDictionary(g => g.Key, g => g.First());

            var selectorBinder = CreateLocalBinder(selectorQueryColumns);

            var boundColumns = new List<BoundOrderByColumn>();

            foreach (var column in node.Columns)
            {
                var selector = column.ColumnSelector;
                var isAscending = column.Modifier == null ||
                                  column.Modifier.Kind == SyntaxKind.AscKeyword;

                // Let's bind the select against the query columns of the first SELECT query
                // we are applied to.

                var boundSelector = selectorBinder.BindOrderBySelector(selectorQueryColumns, column.ColumnSelector);

                // If the expression didn't exist in the query output already, we need to
                // compute it. This is only possible if we don't require selectors being
                // present already.

                if (boundSelector.ComputedValue != null && !selectorsMustBeInInput)
                    QueryState.ComputedProjections.Add(boundSelector.ComputedValue.Value);

                // We need to find the corresponding result query column for the selector.
                // Please note that it might not exist and this is in fact valid. For example,
                // This query is perfectly valid:
                //
                //        SELECT  e.FirstName, e.LastName
                //          FROM  Employees e
                //      ORDER BY  e.FirstName + ' ' + e.LastName
                //
                // However, if the query we are applied to is a combined query, it must exist
                // in the input.

                int columnOrdinal;
                if (!getOrdinalFromSelectorValueSlot.TryGetValue(boundSelector.ValueSlot, out columnOrdinal))
                {
                    columnOrdinal = -1;
                    if (selectorsMustBeInInput)
                        Diagnostics.ReportOrderByItemsMustBeInSelectListIfUnionSpecified(selector.Span);
                }

                var queryColumn = columnOrdinal >= 0
                                      ? resultQueryColumns[columnOrdinal]
                                      : null;
                var valueSlot = queryColumn != null
                                    ? queryColumn.ValueSlot
                                    : boundSelector.ValueSlot;

                // Almost there. Now the only thing left for us to do is getting
                // the associated comparer.

                var baseComparer = BindComparer(valueSlot.Type, selector.Span, DiagnosticId.InvalidDataTypeInOrderBy);
                var comparer = isAscending || baseComparer == null
                                   ? baseComparer
                                   : new NegatedComparer(baseComparer);

                var sortedValue = new BoundSortedValue(valueSlot, comparer);
                var boundColumn = new BoundOrderByColumn(queryColumn, sortedValue);
                Bind(column, boundColumn);
                boundColumns.Add(boundColumn);
            }

            return new BoundOrderByClause(boundColumns);
        }

        private BoundOrderBySelector BindOrderBySelector(IList<QueryColumnInstanceSymbol> queryColumns, ExpressionSyntax selector)
        {
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
            // In other words it can't be a query combined with UNION, EXCEPT, or INTERSECT. However,
            // we don't have to check for that case, our caller is reponsible for doing it.

            // Case (1): Check for positional form.

            var selectorAsLiteral = selector as LiteralExpressionSyntax;
            if (selectorAsLiteral != null)
            {
                var position = selectorAsLiteral.Value as int?;
                if (position != null)
                {
                    var index = position.Value - 1;
                    var indexValid = 0 <= index && index < queryColumns.Count;
                    if (indexValid)
                        return new BoundOrderBySelector(queryColumns[index].ValueSlot, null);

                    // Report that the given position isn't valid.
                    Diagnostics.ReportOrderByColumnPositionIsOutOfRange(selector.Span, position.Value, queryColumns.Count);

                    // And to avoid cascading errors, we'll fake up an invalid slot.
                    var errorSlot = ValueSlotFactory.CreateTemporaryValueSlot(TypeFacts.Missing);
                    return new BoundOrderBySelector(errorSlot, null);
                }
            }

            // Case (2): Check for query column name.

            var selectorAsName = selector as NameExpressionSyntax;
            if (selectorAsName != null)
            {
                var columnSymbols = LookupQueryColumn(selectorAsName.Name).ToArray();
                if (columnSymbols.Length > 0)
                {
                    if (columnSymbols.Length > 1)
                        Diagnostics.ReportAmbiguousColumnInstance(selectorAsName.Name, columnSymbols);

                    var queryColumn = columnSymbols[0];

                    // Since this name isn't bound as a regular expression we simple fake this one up.
                    // This ensures that this name appears to be bound like any other expression.
                    Bind(selectorAsName, new BoundColumnExpression(queryColumn));

                    return new BoundOrderBySelector(queryColumn.ValueSlot, null);
                }
            }

            // Case (3): Bind regular expression.

            var boundSelector = BindExpression(selector);

            if (boundSelector is BoundLiteralExpression)
                Diagnostics.ReportConstantExpressionInOrderBy(selector.Span);

            ValueSlot valueSlot;

            if (TryGetExistingValue(boundSelector, out valueSlot))
                return new BoundOrderBySelector(valueSlot, null);

            valueSlot = ValueSlotFactory.CreateTemporaryValueSlot(boundSelector.Type);
            var computedValue = new BoundComputedValueWithSyntax(selector, boundSelector, valueSlot);
            return new BoundOrderBySelector(valueSlot, computedValue);
        }
    }
}