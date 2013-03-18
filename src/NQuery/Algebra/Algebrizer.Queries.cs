using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private static AlgebraQueryCombinator GetQueryCombinator(BoundQueryCombinator combinator)
        {
            switch (combinator)
            {
                case BoundQueryCombinator.Union:
                    return AlgebraQueryCombinator.Union;
                case BoundQueryCombinator.UnionAll:
                    return AlgebraQueryCombinator.UnionAll;
                case BoundQueryCombinator.Except:
                    return AlgebraQueryCombinator.Except;
                case BoundQueryCombinator.Intersect:
                    return AlgebraQueryCombinator.Intersect;
                default:
                    throw new ArgumentOutOfRangeException("combinator");
            }
        }

        private AlgebraRelation AlgebrizeQuery(BoundQuery node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.SelectQuery:
                    return AlgebrizeSelectQuery((BoundSelectQuery)node);
                case BoundNodeKind.CombinedQuery:
                    return AlgebrizeCombinedQuery((BoundCombinedQuery)node);
                case BoundNodeKind.OrderedQuery:
                    return AlgebrizeOrderedQuery((BoundOrderedQuery)node);
                default:
                    throw new ArgumentException();
            }
        }

        private AlgebraRelation AlgebrizeSelectQuery(BoundSelectQuery node)
        {
            var algebrizedFrom = AlgebrizeFrom(node.FromClause);
            var algebrizedWhere = AlgebrizeFilter(algebrizedFrom, node.WhereClause);
            var algebrizedGroups = AlgebrizeCompute(algebrizedWhere, node.Groups);
            var algebrizedGroupByAndAggregation = AlgebrizeGroupByAndAggregation(algebrizedGroups, node.Groups, node.Aggregates);
            var algebrizedHaving = AlgebrizeFilter(algebrizedGroupByAndAggregation, node.HavingClause);
            var algebrizedProjections = AlgebrizeCompute(algebrizedHaving, node.Projections);
            var algebrizedOrderBy = AlgebrizeOrderBy(algebrizedProjections, node.OrderByClause);
            var algebrizedTop = AlgebrizeTop(algebrizedOrderBy, node.Top, node.WithTies ? node.OrderByClause.Columns : null);
            var algebrizedProject = AlgebrizeProject(algebrizedTop, node.OutputColumns);
            return algebrizedProject;
        }

        private AlgebraRelation AlgebrizeFrom(BoundTableReference boundTableReference)
        {
            return boundTableReference == null
                       ? new AlgebraConstantNode()
                       : AlgebrizeTableReference(boundTableReference);
        }

        private AlgebraRelation AlgebrizeFilter(AlgebraRelation input, BoundExpression condition)
        {
            if (condition == null)
                return input;

            var algebrizedCondition = AlgebrizeExpression(condition);
            return new AlgebraFilterNode(input, algebrizedCondition);
        }

        private AlgebraRelation AlgebrizeCompute(AlgebraRelation input, IEnumerable<Tuple<BoundExpression, ValueSlot>> projections)
        {
            var definedValues = (from c in projections
                                 let expression = c.Item1
                                 let valueSlot = c.Item2
                                 where !(expression is BoundValueSlotExpression)
                                 let algebraExpression = AlgebrizeExpression(expression)
                                 select new AlgebraComputedValue(algebraExpression, valueSlot)).ToArray();

            if (definedValues.Length == 0)
                return input;

            return new AlgebraComputeNode(input, definedValues);
        }

        private AlgebraRelation AlgebrizeGroupByAndAggregation(AlgebraRelation input, ICollection<Tuple<BoundExpression, ValueSlot>> groups, ICollection<Tuple<BoundAggregateExpression, ValueSlot>> aggregates)
        {
            if (groups.Count == 0 && aggregates.Count == 0)
                return input;

            // TODO: Emit compute for GROUPs

            var columns = new ReadOnlyCollection<ValueSlot>(groups.Select(g => g.Item2).ToArray());

            var algberizedAggregates = (from t in aggregates
                                        let aggregate = t.Item1.Aggregate
                                        let argument = AlgebrizeExpression(t.Item1.Argument)
                                        let output = t.Item2
                                        select new AlgebraAggregateDefinition(output, aggregate, argument)).ToArray();

            return new AlgebraGroupByAndAggregation(input, columns, algberizedAggregates);
        }

        private AlgebraRelation AlgebrizeOrderBy(AlgebraRelation input, BoundOrderByClause orderByClause)
        {
            if (orderByClause == null)
                return input;

            var expressions = orderByClause.Columns.Select(c => c.ValueSlot).ToArray();
            var comparers = orderByClause.Columns.Select(c => c.Comparer).ToArray();
            return new AlgebraSortNode(input, expressions, comparers);
        }

        private AlgebraRelation AlgebrizeTop(AlgebraRelation input, int? top, ReadOnlyCollection<BoundOrderByColumn> orderByColumns)
        {
            if (top == null)
                return input;

            var tieEntries = orderByColumns == null
                                 ? new ValueSlot[0]
                                 : orderByColumns.Select(c => c.ValueSlot).ToArray();
            var tieComparer = orderByColumns == null
                                  ? new IComparer[0]
                                  : orderByColumns.Select(c => c.Comparer).ToArray();
            return new AlgebraTopNode(input, top.Value, tieEntries, tieComparer);
        }

        private AlgebraRelation AlgebrizeProject(AlgebraRelation input, IEnumerable<QueryColumnInstanceSymbol> outputColumns)
        {
            var output = outputColumns.Select(c => c.ValueSlot).ToArray();
            return new AlgebraProjectNode(input, output);
        }

        private AlgebraRelation AlgebrizeCombinedQuery(BoundCombinedQuery node)
        {
            var left = AlgebrizeQuery(node.Left);
            var right = AlgebrizeQuery(node.Right);
            var combinator = GetQueryCombinator(node.Combinator);
            var outputValueSlots = node.OutputColumns.Select(c => c.ValueSlot).ToArray();
            return new AlgebraCombinedQuery(left, right, combinator, outputValueSlots);
        }

        private AlgebraRelation AlgebrizeOrderedQuery(BoundOrderedQuery node)
        {
            var input = AlgebrizeQuery(node.Input);
            var valueSlots = node.Columns.Select(s => s.ValueSlot).ToArray();
            var comparers = node.Columns.Select(c => c.Comparer).ToArray();
            return new AlgebraSortNode(input, valueSlots, comparers);
        }
    }
}