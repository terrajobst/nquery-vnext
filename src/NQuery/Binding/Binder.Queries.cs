using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            get { return Parent == null ? null : Parent.QueryState; }
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
            return columnExpression?.Symbol.Name ?? string.Empty;
        }

        private BoundQueryState FindQueryState(TableInstanceSymbol tableInstanceSymbol)
        {
            var queryState = QueryState;
            while (queryState != null)
            {
                if (queryState.IntroducedTables.ContainsKey(tableInstanceSymbol))
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
                var candidates = queryState.AccessibleComputedValues
                                           .Concat(queryState.ComputedAggregates);
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
            // TODO: This is not entirely true. EXISTS (SELECT * FROM GROUP BY ...) is considered valid.
            //
            // For example, the following query is valid:
            //
            //      SELECT  *
            //      FROM    Employees e
            //      WHERE   EXISTS (
            //                  SELECT  *
            //                  FROM    Orders o
            //                              INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
            //                  WHERE   o.EmployeeID = e.EmployeeID
            //                  GROUP   BY o.OrderID
            //                  HAVING  SUM(od.UnitPrice * od.Quantity) > 12000
            //              )
            //
            // Please note that explicitly expanding the * is not valid though. For example the
            // following query should generate an error:
            //
            //      SELECT  *
            //      FROM    Employees e
            //      WHERE   EXISTS (
            //                  SELECT  od.UnitPrice,
            //                          od.Quantity
            //                  FROM    Orders o
            //                              INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
            //                  WHERE   o.EmployeeID = e.EmployeeID
            //                  GROUP   BY o.OrderID
            //                  HAVING  SUM(od.UnitPrice * od.Quantity) > 12000
            //              )
            //
            // This also includes the qualified asterisk. For example, this query is also invalid:
            //
            //      SELECT  *
            //      FROM    Employees e
            //      WHERE   EXISTS (
            //                  SELECT  o.*
            //                  FROM    Orders o
            //                              INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
            //                  WHERE   o.EmployeeID = e.EmployeeID
            //                  GROUP   BY o.OrderID
            //                  HAVING  SUM(od.UnitPrice * od.Quantity) > 12000
            //              )
            //
            // Also SELECT * is only considered valid if the direct parent is EXISTS. For example,
            // the following query is invalid:
            //
            //      SELECT  *
            //      FROM    Employees e
            //      WHERE   EXISTS (
            //                  SELECT  *
            //                  FROM    Orders o
            //                              INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
            //                  WHERE   o.EmployeeID = e.EmployeeID
            //                  GROUP   BY o.OrderID
            //                  HAVING  SUM(od.UnitPrice * od.Quantity) > 12000
            //
            //                  UNION ALL
            //
            //                  SELECT  *
            //                  FROM    Orders o
            //                              INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
            //              )

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
                                          where QueryState.IntroducedTables.ContainsKey(t.Item2.TableInstance)
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

        private IComparer BindComparer(TextSpan diagnosticSpan, Type type, DiagnosticId errorId)
        {
            var missingComparer = Comparer.Default;

            if (type.IsError())
                return missingComparer;

            var comparer = LookupComparer(type);
            if (comparer == null)
            {
                comparer = missingComparer;
                Diagnostics.Report(diagnosticSpan, errorId, type.ToDisplayName());
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
                case SyntaxKind.UnionQuery:
                    return BindUnionQuery((UnionQuerySyntax)node);

                case SyntaxKind.IntersectQuery:
                    return BindIntersectQuery((IntersectQuerySyntax)node);

                case SyntaxKind.ExceptQuery:
                    return BindExceptQuery((ExceptQuerySyntax)node);

                case SyntaxKind.OrderedQuery:
                    return BindOrderedQuery((OrderedQuerySyntax)node);

                case SyntaxKind.ParenthesizedQuery:
                    return BindParenthesizedQuery((ParenthesizedQuerySyntax)node);

                case SyntaxKind.CommonTableExpressionQuery:
                    return BindCommonTableExpressionQuery((CommonTableExpressionQuerySyntax)node);

                case SyntaxKind.SelectQuery:
                    return BindSelectQuery((SelectQuerySyntax)node);

                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            var diagnosticSpan = node.UnionKeyword.Span;
            var isUnionAll = node.AllKeyword != null;
            var queries = BindUnionInputs(node);
            return BindUnionOrUnionAllQuery(diagnosticSpan, isUnionAll, queries);
        }

        private BoundQuery BindUnionOrUnionAllQuery(TextSpan diagnosticSpan, bool isUnionAll, ImmutableArray<BoundQuery> queries)
        {
            var firstQuery = queries.First();

            if (queries.Length == 1 && isUnionAll)
                return firstQuery;

            foreach (var otherQuery in queries.Skip(1))
            {
                if (otherQuery.OutputColumns.Length != firstQuery.OutputColumns.Length)
                    Diagnostics.ReportDifferentExpressionCountInBinaryQuery(diagnosticSpan);
            }

            var inputs = BindToCommonTypes(diagnosticSpan, queries);
            var columnCount = queries.Select(q => q.OutputColumns.Length).Min();
            var outputValues = inputs.First()
                .GetOutputValues()
                .Take(columnCount)
                .Select(v => ValueSlotFactory.CreateTemporary(v.Type))
                .ToImmutableArray();
            var definedValues = outputValues.Select((valueSlot, columnIndex) => new BoundUnifiedValue(valueSlot, inputs.Select(input => input.GetOutputValues().ElementAt(columnIndex))));
            var outputColumns = BindOutputColumns(queries.First().OutputColumns, outputValues);
            var comparers = isUnionAll
                ? Enumerable.Empty<IComparer>()
                : outputColumns.Select(c => BindComparer(diagnosticSpan, c.Type, DiagnosticId.InvalidDataTypeInUnion));

            var relation = new BoundUnionRelation(isUnionAll, inputs, definedValues, comparers);
            return new BoundQuery(relation, outputColumns);
        }

        private ImmutableArray<BoundQuery> BindUnionInputs(UnionQuerySyntax node)
        {
            var queries = new List<QuerySyntax>();
            FlattenUnionQueries(queries, node);

            return queries.Select(BindQuery).ToImmutableArray();
        }

        private static void FlattenUnionQueries(ICollection<QuerySyntax> receiver, UnionQuerySyntax node)
        {
            var leftAsUnion = node.LeftQuery as UnionQuerySyntax;
            if (leftAsUnion != null && HasSameUnionKind(node, leftAsUnion))
                FlattenUnionQueries(receiver, leftAsUnion);
            else
                receiver.Add(node.LeftQuery);

            var rightAsUnion = node.RightQuery as UnionQuerySyntax;
            if (rightAsUnion != null && HasSameUnionKind(node, rightAsUnion))
                FlattenUnionQueries(receiver, rightAsUnion);
            else
                receiver.Add(node.RightQuery);
        }

        private static bool HasSameUnionKind(UnionQuerySyntax left, UnionQuerySyntax right)
        {
            return (left.AllKeyword == null) == (right.AllKeyword == null);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            var diagnosticSpan = node.IntersectKeyword.Span;
            var left = node.LeftQuery;
            var right = node.RightQuery;
            return BindIntersectOrExceptQuery(diagnosticSpan, true, left, right);
        }

        private BoundQuery BindExceptQuery(ExceptQuerySyntax node)
        {
            var diagnosticSpan = node.ExceptKeyword.Span;
            var left = node.LeftQuery;
            var right = node.RightQuery;
            return BindIntersectOrExceptQuery(diagnosticSpan, false, left, right);
        }

        private BoundQuery BindIntersectOrExceptQuery(TextSpan diagnosticSpan, bool isIntersect, QuerySyntax left, QuerySyntax right)
        {
            var boundLeft = BindQuery(left);
            var boundRight = BindQuery(right);
            var columns = boundLeft.OutputColumns;

            if (boundLeft.OutputColumns.Length != boundRight.OutputColumns.Length)
                Diagnostics.ReportDifferentExpressionCountInBinaryQuery(diagnosticSpan);

            BoundRelation leftInput;
            BoundRelation rightInput;
            ImmutableArray<ValueSlot> leftValues;
            ImmutableArray<ValueSlot> rightValues;
            BindToCommonTypes(diagnosticSpan, boundLeft, boundRight, out leftInput, out rightInput, out leftValues,
                out rightValues);

            var outputValues = leftValues;
            var outputColumns = BindOutputColumns(columns, outputValues);

            var diagnosticId = isIntersect
                ? DiagnosticId.InvalidDataTypeInIntersect
                : DiagnosticId.InvalidDataTypeInExcept;

            var comparers = outputColumns.Select(c => BindComparer(diagnosticSpan, c.Type, diagnosticId));

            var relation = new BoundIntersectOrExceptRelation(isIntersect, leftInput, rightInput, comparers);
            return new BoundQuery(relation, outputColumns);
        }

        private void BindToCommonTypes(TextSpan diagnosticSpan, BoundQuery left, BoundQuery right, out BoundRelation newLeft, out BoundRelation newRight, out ImmutableArray<ValueSlot> leftValues, out ImmutableArray<ValueSlot> rightValues)
        {
            var columnCount = Math.Min(left.OutputColumns.Length, right.OutputColumns.Length);

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
                BindToCommonType(diagnosticSpan, leftValue, rightValue, out convertedLeft, out convertedRight);

                if (convertedLeft == null)
                {
                    leftOutputs.Add(leftValue);
                }
                else
                {
                    var newLeftValue = ValueSlotFactory.CreateTemporary(convertedLeft.Type);
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
                    var newRightValue = ValueSlotFactory.CreateTemporary(convertedRight.Type);
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

            leftValues = leftOutputs.ToImmutableArray();
            rightValues = rightOutputs.ToImmutableArray();
        }

        private ImmutableArray<BoundRelation> BindToCommonTypes(TextSpan diagnosticSpan, ImmutableArray<BoundQuery> queries)
        {
            var columnCount = queries.Select(q => q.OutputColumns.Length).Min();
            var computations = new List<BoundComputedValue>[queries.Length];

            var outputs = new List<ValueSlot>[queries.Length];
            for (var i = 0; i < outputs.Length; i++)
                outputs[i] = new List<ValueSlot>();

            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var expressions = queries.Select(q => new BoundValueSlotExpression(q.OutputColumns[columnIndex].ValueSlot)).OfType<BoundExpression>().ToImmutableArray();
                var convertedExpressions = BindToCommonType(expressions, diagnosticSpan);

                for (var queryIndex = 0; queryIndex < expressions.Length; queryIndex++)
                {
                    var input = expressions[queryIndex];
                    var converted = convertedExpressions[queryIndex];

                    if (input == converted)
                    {
                        // Nothing to do.

                        var originalValueSlot = queries[queryIndex].OutputColumns[columnIndex].ValueSlot;
                        outputs[queryIndex].Add(originalValueSlot);
                    }
                    else
                    {
                        if (computations[queryIndex] == null)
                            computations[queryIndex] = new List<BoundComputedValue>();

                        var computedValueSlot = ValueSlotFactory.CreateTemporary(converted.Type);
                        var computedValue = new BoundComputedValue(converted, computedValueSlot);
                        computations[queryIndex].Add(computedValue);

                        outputs[queryIndex].Add(computedValueSlot);
                    }
                }
            }

            var result = new List<BoundRelation>();

            for (var queryIndex = 0; queryIndex < queries.Length; queryIndex++)
            {
                var computedValues = computations[queryIndex];
                var queryRelation = queries[queryIndex].Relation;
                if (computedValues == null)
                {
                    result.Add(queryRelation);
                }
                else
                {
                    var computeRelation = new BoundComputeRelation(queryRelation, computedValues);
                    var projectedOutputs = outputs[queryIndex];
                    var projectRelation = new BoundProjectRelation(computeRelation, projectedOutputs);
                    result.Add(projectRelation);
                }
            }

            return result.ToImmutableArray();
        }

        private static ImmutableArray<QueryColumnInstanceSymbol> BindOutputColumns(ImmutableArray<QueryColumnInstanceSymbol> inputColumns, ImmutableArray<ValueSlot> outputValues)
        {
            var result = new List<QueryColumnInstanceSymbol>(inputColumns.Length);

            for (var i = 0; i < inputColumns.Length; i++)
            {
                if (i >= outputValues.Length)
                    break;

                var queryColumn = inputColumns[i];
                var valueSlot = outputValues[i];

                var resultColumn = queryColumn.ValueSlot == valueSlot
                    ? queryColumn
                    : new QueryColumnInstanceSymbol(queryColumn.Name, valueSlot);

                result.Add(resultColumn);
            }

            return result.ToImmutableArray();
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            // Depending on the query the ORDER BY was applied on, the binding rules
            // differ.
            //
            // (1) If the query is applied to a SELECT query, then ORDER BY may have
            //     more expressions than the underlying SELECT list.
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

            var selectQuery = node.GetAppliedSelectQuery();
            if (selectQuery != null)
            {
                // This is case (1). We bind the select query and pass in ourselves.
                var boundQuery = BindSelectQuery(selectQuery, node);

                // Since we've potentially skipped a bunch of parenthesized queries
                // our regular binder hasn't seen those queries. In order to make
                // sure that subsequent requests for GetBoundNode() will return a
                // valid instance we must manually bind them to the bound query.
                var queries = selectQuery.AncestorsAndSelf().OfType<QuerySyntax>().TakeWhile(n => n != node);
                foreach (var queryNodes in queries)
                    Bind(queryNodes, boundQuery);

                return boundQuery;
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
            var outputQueryColumns = query.OutputColumns;
            var orderByClause = binder.BindOrderByClause(node, inputQueryColumns, outputQueryColumns);

            var relation = new BoundSortRelation(false, query.Relation, orderByClause.Columns.Select(c => c.ComparedValue).ToImmutableArray());
            return new BoundQuery(relation, outputQueryColumns);
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

            foreach (var commonTableExpression in node.CommonTableExpressions)
            {
                var boundCommonTableExpression = currentBinder.BindCommonTableExpression(commonTableExpression);

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
            var name = commonTableExpression.Name.ValueText;
            var symbol = new CommonTableExpressionSymbol(name, s =>
            {
                var binder = CreateLocalBinder(s);
                var boundQuery = binder.BindQuery(commonTableExpression.Query);
                var columns = binder.BindCommonTableExpressionColumns(commonTableExpression, boundQuery);
                return (boundQuery, columns);
            });

            return new BoundCommonTableExpression(symbol);
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
                    return BindErrorCommonTableExpression(commonTableExpression);
            }

            var members = new List<QuerySyntax>();
            FlattenUnionQueries(members, rootQuery);

            var anchorMembers = new List<QuerySyntax>();
            var recursiveMembers = new List<QuerySyntax>();

            foreach (var member in members)
            {
                if (IsRecursive(commonTableExpression, member))
                {
                    recursiveMembers.Add(member);
                }
                else
                {
                    if (recursiveMembers.Any())
                        Diagnostics.ReportCteContainsUnexpectedAnchorMember(commonTableExpression.Name);
                    anchorMembers.Add(member);
                }
            }

            // Ensure we have at least one anchor.

            if (anchorMembers.Count == 0)
            {
                Diagnostics.ReportCteDoesNotHaveAnchorMember(commonTableExpression.Name);
                return BindErrorCommonTableExpression(commonTableExpression);
            }

            var symbol = new CommonTableExpressionSymbol(commonTableExpression.Name.ValueText, s =>
            {
                var binder = CreateLocalBinder(s);
                var boundAnchor = binder.BindCommonTableExpressionAnchorMember(commonTableExpression, anchorMembers);
                var columns = binder.BindCommonTableExpressionColumns(commonTableExpression, boundAnchor);
                return (boundAnchor, columns);
            }, s =>
            {
                var binder = CreateLocalBinder(s);
                return binder.BindCommonTableExpressionRecursiveMembers(commonTableExpression, s, recursiveMembers);
            });

            return new BoundCommonTableExpression(symbol);
        }

        private BoundQuery BindCommonTableExpressionAnchorMember(CommonTableExpressionSyntax commonTableExpression, IEnumerable<QuerySyntax> anchorMembers)
        {
            var diagnosticSpan = commonTableExpression.Name.Span;
            var boundAnchorMembers = anchorMembers.Select(BindQuery).ToImmutableArray();
            return BindUnionOrUnionAllQuery(diagnosticSpan, true, boundAnchorMembers);
        }

        private ImmutableArray<BoundQuery> BindCommonTableExpressionRecursiveMembers(CommonTableExpressionSyntax commonTableExpression, CommonTableExpressionSymbol symbol, IEnumerable<QuerySyntax> recursiveMembers)
        {
            return recursiveMembers.Select(r => BindCommonTableExpressionRecursiveMember(commonTableExpression, symbol, r))
                                   .ToImmutableArray();
        }

        private BoundQuery BindCommonTableExpressionRecursiveMember(CommonTableExpressionSyntax commonTableExpression, CommonTableExpressionSymbol symbol, QuerySyntax recursiveMember)
        {
            var boundAnchorQuery = symbol.Anchor;
            var name = commonTableExpression.Name;
            var diagnosticSpan = name.Span;
            var boundRecursiveMember = BindQuery(recursiveMember);

            if (boundRecursiveMember.OutputColumns.Length != boundAnchorQuery.OutputColumns.Length)
                Diagnostics.ReportDifferentExpressionCountInBinaryQuery(diagnosticSpan);

            var columnCount = Math.Min(boundAnchorQuery.OutputColumns.Length, boundRecursiveMember.OutputColumns.Length);
            for (var i = 0; i < columnCount; i++)
            {
                var anchorColumn = boundAnchorQuery.OutputColumns[i];
                var recursiveColumn = boundRecursiveMember.OutputColumns[i];
                if (anchorColumn.Type != recursiveColumn.Type)
                    Diagnostics.ReportCteHasTypeMismatchBetweenAnchorAndRecursivePart(diagnosticSpan, anchorColumn.Name, recursiveColumn.Name);
            }

            var checker = new RecursiveCommonTableExpressionChecker(commonTableExpression, Diagnostics, symbol);
            checker.VisitRelation(boundRecursiveMember.Relation);

            return boundRecursiveMember;
        }

        private static BoundCommonTableExpression BindErrorCommonTableExpression(CommonTableExpressionSyntax commonTableExpression)
        {
            var symbol = new CommonTableExpressionSymbol(
                commonTableExpression.Name.ValueText,
                s => ((BoundQuery) null, ImmutableArray<ColumnSymbol>.Empty),
                s => ImmutableArray<BoundQuery>.Empty
            );

            return new BoundCommonTableExpression(symbol);
        }

        private ImmutableArray<ColumnSymbol> BindCommonTableExpressionColumns(CommonTableExpressionSyntax commonTableExpression, BoundQuery boundQuery)
        {
            // Now let's figure out the column names we want to give result.

            var specifiedColumnNames = commonTableExpression.ColumnNameList?.ColumnNames;
            var queryColumns = boundQuery.OutputColumns;

            if (specifiedColumnNames == null)
            {
                // If the CTE doesn't have a column list, the query must have names for all columns.

                for (var i = 0; i < queryColumns.Length; i++)
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
                var actualCount = queryColumns.Length;

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
                ? queryColumns.Length
                : Math.Min(specifiedColumnNames.Count, queryColumns.Length);

            var columnNames = specifiedColumnNames == null
                ? queryColumns.Select(c => c.Name)
                : specifiedColumnNames.Select(t => t.Identifier.ValueText);

            var columns = queryColumns.Take(columnCount)
                                      .Zip(columnNames, (c, n) => new ColumnSymbol(n, c.Type))
                                      .Where(c => !string.IsNullOrEmpty(c.Name))
                                      .ToImmutableArray();

            var uniqueColumnNames = new HashSet<string>();
            foreach (var column in columns.Where(c => !uniqueColumnNames.Add(c.Name)))
                Diagnostics.ReportCteHasDuplicateColumnName(commonTableExpression.Name, column.Name);

            return columns;
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node)
        {
            return BindSelectQuery(node, null);
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node, OrderedQuerySyntax orderedQueryNode)
        {
            var queryBinder = CreateQueryBinder();

            var fromClause = queryBinder.BindFromClause(node.FromClause);

            var fromAwareBinder = fromClause == null
                                      ? queryBinder
                                      : _sharedBinderState.BinderFromBoundNode[fromClause];

            var whereClause = fromAwareBinder.BindWhereClause(node.WhereClause);

            var groupByClause = fromAwareBinder.BindGroupByClause(node.GroupByClause);

            var havingClause = fromAwareBinder.BindHavingClause(node.HavingClause);

            var selectColumns = fromAwareBinder.BindSelectColumns(node.SelectClause.Columns);

            var outputColumns = selectColumns.Select(s => s.Column).ToImmutableArray();

            var orderByClause = fromAwareBinder.BindOrderByClause(orderedQueryNode, outputColumns, outputColumns);

            queryBinder.EnsureAllColumnReferencesAreLegal(node, orderedQueryNode);

            var aggregates = (from t in queryBinder.QueryState.ComputedAggregates
                              let expression = (BoundAggregateExpression)t.Expression
                              select new BoundAggregatedValue(t.Result, expression.Aggregate, expression.Aggregatable, expression.Argument)).ToImmutableArray();

            var groups = groupByClause == null
                ? ImmutableArray<BoundComparedValue>.Empty
                : groupByClause.Groups;

            var projections = (from t in queryBinder.QueryState.ComputedProjections
                               select new BoundComputedValue(t.Expression, t.Result)).ToImmutableArray();

            var distinctKeyword = node.SelectClause.DistinctAllKeyword;
            var isDistinct = distinctKeyword != null &&
                             distinctKeyword.Kind == SyntaxKind.DistinctKeyword;

            var distinctComparer = isDistinct
                ? BindDistinctComparers(node.SelectClause.Columns, outputColumns)
                : ImmutableArray<IComparer>.Empty;

            ImmutableArray<BoundComparedValue> distinctSortValues;

            if (!isDistinct || orderByClause == null)
            {
                distinctSortValues = ImmutableArray<BoundComparedValue>.Empty;
            }
            else
            {
                var outputValueSet = new HashSet<ValueSlot>(outputColumns.Select(c => c.ValueSlot));

                for (var i = 0; i < orderByClause.Columns.Length; i++)
                {
                    var column = orderedQueryNode.Columns[i];
                    var boundColumn = orderByClause.Columns[i];
                    var orderedValue = boundColumn.ComparedValue.ValueSlot;

                    if (!outputValueSet.Contains(orderedValue))
                        Diagnostics.ReportOrderByItemsMustBeInSelectListIfDistinctSpecified(column.Span);
                }

                var orderByValueSet = new HashSet<ValueSlot>(orderByClause.Columns.Select(c => c.ComparedValue.ValueSlot));
                distinctSortValues = outputColumns.Select((c, i) => new BoundComparedValue(c.ValueSlot, distinctComparer[i]))
                                                 .Where(s => !orderByValueSet.Contains(s.ValueSlot))
                                                 .ToImmutableArray();
            }

            // NOTE: We rely on the fact that the parser already ensured the argument to TOP is a valid integer
            //       literal. Thus, we can simply ignore the case where topClause.Value.Value cannot be casted
            //       to an int -- the parser added the diagnostics already. However, we cannot perform a hard
            //       cast because we also bind input the parser reported errors for.

            var topClause = node.SelectClause.TopClause;
            var top = topClause == null ? null : topClause.Value.Value as int?;
            var withTies = topClause != null && topClause.WithKeyword != null;

            if (withTies && orderedQueryNode == null)
                Diagnostics.ReportTopWithTiesRequiresOrderBy(topClause.Span);

            // Putting it together

            var fromRelation = fromClause ?? new BoundConstantRelation();

            var whereRelation = whereClause == null
                ? fromRelation
                : new BoundFilterRelation(fromRelation, whereClause);

            var computedGroups = queryBinder.QueryState
                                            .ComputedGroupings
                                            .Where(g => !(g.Expression is BoundValueSlotExpression))
                                            .Select(g => new BoundComputedValue(g.Expression, g.Result))
                                            .ToImmutableArray();
            var groupComputeRelation = !computedGroups.Any()
                                            ? whereRelation
                                            : new BoundComputeRelation(whereRelation, computedGroups);

            var groupByAndAggregationRelation = !groups.Any() && !aggregates.Any()
                ? groupComputeRelation
                : new BoundGroupByAndAggregationRelation(groupComputeRelation, groups, aggregates);

            var havingRelation = havingClause == null
                ? groupByAndAggregationRelation
                : new BoundFilterRelation(groupByAndAggregationRelation, havingClause);

            var selectComputeRelation = projections.Length == 0
                ? havingRelation
                : new BoundComputeRelation(havingRelation, projections);

            var sortedValues = orderByClause == null
                ? ImmutableArray<BoundComparedValue>.Empty
                : orderByClause.Columns.Select(c => c.ComparedValue).Concat(distinctSortValues).ToImmutableArray();

            var sortRelation = sortedValues.IsEmpty
                ? selectComputeRelation
                : new BoundSortRelation(isDistinct, selectComputeRelation, sortedValues);

            var tieEntries = top == null || sortedValues.IsEmpty || !withTies
                ? ImmutableArray<BoundComparedValue>.Empty
                : sortedValues;

            var topRelation = top == null
                ? sortRelation
                : new BoundTopRelation(sortRelation, top.Value, tieEntries);

            var projectRelation = new BoundProjectRelation(topRelation, outputColumns.Select(c => c.ValueSlot).ToImmutableArray());

            BoundRelation distinctRelation;

            if (!isDistinct || orderByClause != null)
            {
                distinctRelation = projectRelation;
            }
            else
            {
                var distinctValues = from column in outputColumns
                                     let comparer = BindComparer(distinctKeyword.Span, column.Type, DiagnosticId.InvalidDataTypeInSelectDistinct)
                                     select new BoundComparedValue(column.ValueSlot, comparer);

                distinctRelation = new BoundGroupByAndAggregationRelation(projectRelation, distinctValues, Enumerable.Empty<BoundAggregatedValue>());
            }

            return new BoundQuery(distinctRelation, outputColumns);
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
                        throw ExceptionBuilder.UnexpectedValue(node.Kind);
                }
            }

            QueryState.AccessibleComputedValues.AddRange(QueryState.ComputedProjections);

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
                valueSlot = ValueSlotFactory.CreateTemporary(boundExpression.Type);
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
            var symbols = LookupTableInstance(tableName).ToImmutableArray();

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
            var tableInstances = LookupTableInstances().ToImmutableArray();

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

            var columnInstances = tableInstances.SelectMany(t => t.ColumnInstances).ToImmutableArray();
            return new BoundWildcardSelectColumn(null, columnInstances);
        }

        private static IEnumerable<BoundSelectColumn> BindSelectColumns(BoundWildcardSelectColumn selectColumn)
        {
            return selectColumn.QueryColumns.Select(c => new BoundSelectColumn(c));
        }

        private ImmutableArray<IComparer> BindDistinctComparers(IReadOnlyList<SelectColumnSyntax> columns, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            var comparers = new List<IComparer>();

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                var columnType = outputColumns[columnIndex].ValueSlot.Type;
                var comparer = BindComparer(column.Span, columnType, DiagnosticId.InvalidDataTypeInSelectDistinct);
                comparers.Add(comparer);
            }

            return comparers.ToImmutableArray();
        }

        private BoundRelation BindFromClause(FromClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundNode = BindFromClauseInternal(node);
            var binder = CreateLocalBinder(boundNode.GetDeclaredTableInstances());

            _sharedBinderState.BoundNodeFromSyntaxNode.Add(node, boundNode);
            _sharedBinderState.BinderFromBoundNode[boundNode] = binder;

            // Ensure that there are no duplicates.

            var introducedTables = QueryState.IntroducedTables;
            var lookup = introducedTables.Values.ToLookup(t => t.ValueText, StringComparer.OrdinalIgnoreCase);

            foreach (var name in introducedTables.Values)
            {
                var matches = lookup[name.ValueText];

                if (name.IsQuotedIdentifier())
                    matches = matches.Where(t => string.Equals(t.ValueText, name.ValueText, StringComparison.Ordinal));

                var isDuplicate = matches.Skip(1).Any();
                if (isDuplicate)
                    Diagnostics.ReportDuplicateTableRefInFrom(name);
            }

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
                    lastTableReference = new BoundJoinRelation(BoundJoinType.Inner, lastTableReference, boundTableReference, null, null, null);
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

            var groups = new List<BoundComparedValue>(groupByClause.Columns.Count);

            foreach (var column in groupByClause.Columns)
            {
                var expression = column.Expression;
                var boundExpression = groupByBinder.BindExpression(expression);
                var expressionType = boundExpression.Type;

                // TODO: We need to ensure every expression references at least one column that is not an outer reference.

                var comparer = BindComparer(expression.Span, expressionType, DiagnosticId.InvalidDataTypeInGroupBy);

                ValueSlot valueSlot;
                if (!TryGetExistingValue(boundExpression, out valueSlot))
                    valueSlot = ValueSlotFactory.CreateTemporary(expressionType);

                // NOTE: Keep this outside the if check because we assume all groups are recorded
                //       -- independent from whether they are based on existing values or not.
                QueryState.ComputedGroupings.Add(new BoundComputedValueWithSyntax(expression, boundExpression, valueSlot));

                var group = new BoundComparedValue(valueSlot, comparer);
                groups.Add(group);
            }

            QueryState.AccessibleComputedValues.AddRange(QueryState.ComputedAggregates);
            QueryState.AccessibleComputedValues.AddRange(QueryState.ComputedGroupings);

            return new BoundGroupByClause(groups.ToImmutableArray());
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

        private BoundOrderByClause BindOrderByClause(OrderedQuerySyntax node, ImmutableArray<QueryColumnInstanceSymbol> selectorQueryColumns, ImmutableArray<QueryColumnInstanceSymbol> resultQueryColumns)
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
            // actual query. In order to map from the selectors to the result query columns
            // we will use their ordinals.

            var selectorsMustBeInInput = selectorQueryColumns != resultQueryColumns;
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

                // Let's bind the selector against the query columns of the first SELECT query
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

                var baseComparer = BindComparer(selector.Span, valueSlot.Type, DiagnosticId.InvalidDataTypeInOrderBy);
                var comparer = isAscending || baseComparer == null
                                   ? baseComparer
                                   : new NegatedComparer(baseComparer);

                var sortedValue = new BoundComparedValue(valueSlot, comparer);
                var boundColumn = new BoundOrderByColumn(queryColumn, sortedValue);
                Bind(column, boundColumn);
                boundColumns.Add(boundColumn);
            }

            return new BoundOrderByClause(boundColumns);
        }

        private BoundOrderBySelector BindOrderBySelector(ImmutableArray<QueryColumnInstanceSymbol> queryColumns, ExpressionSyntax selector)
        {
            // Although ORDER BY can contain arbitrary expressions, there are special rules to how those
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
            // we don't have to check for that case, our caller is responsible for doing it.

            // Case (1): Check for positional form.

            var selectorAsLiteral = selector as LiteralExpressionSyntax;
            if (selectorAsLiteral != null)
            {
                var position = selectorAsLiteral.Value as int?;
                if (position != null)
                {
                    var index = position.Value - 1;
                    var indexValid = 0 <= index && index < queryColumns.Length;
                    if (indexValid)
                        return new BoundOrderBySelector(queryColumns[index].ValueSlot, null);

                    // Report that the given position isn't valid.
                    Diagnostics.ReportOrderByColumnPositionIsOutOfRange(selector.Span, position.Value, queryColumns.Length);

                    // And to avoid cascading errors, we'll fake up an invalid slot.
                    var errorSlot = ValueSlotFactory.CreateTemporary(TypeFacts.Missing);
                    return new BoundOrderBySelector(errorSlot, null);
                }
            }

            // Case (2): Check for query column name.

            var selectorAsName = selector as NameExpressionSyntax;
            if (selectorAsName != null)
            {
                var columnSymbols = LookupQueryColumn(selectorAsName.Name).ToImmutableArray();
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

            valueSlot = ValueSlotFactory.CreateTemporary(boundSelector.Type);
            var computedValue = new BoundComputedValueWithSyntax(selector, boundSelector, valueSlot);
            return new BoundOrderBySelector(valueSlot, computedValue);
        }
    }
}