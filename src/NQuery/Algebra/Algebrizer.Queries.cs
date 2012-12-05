using System;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Binding;
using NQuery.BoundNodes;

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
                default:
                    throw new ArgumentException();
            }
        }

        private AlgebraRelation AlgebrizeSelectQuery(BoundSelectQuery node)
        {
            var algebrizedFrom = AlgebrizeFrom(node.FromClause);
            var algebrizedWhere = AlgebrizeFilter(algebrizedFrom, node.WhereClause);
            var algebrizedLocalValues = AlgebrizeLocalValues(algebrizedWhere, node.LocalValues);
            var algebrizedGroupByAndAggregation = AlgebrizeGroupByAndAggregation(algebrizedLocalValues, node.GroupByClause);
            var algebrizedHaving = AlgebrizeFilter(algebrizedGroupByAndAggregation, node.HavingClause);
            var algebrizedOrderBy = AlgebrizeOrderBy(algebrizedHaving, node.OrderByClause);
            var algebrizedTop = AlgebrizeTop(algebrizedOrderBy, node.Top, node.WithTies);
            return algebrizedTop;
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

        private AlgebraRelation AlgebrizeLocalValues(AlgebraRelation input, ReadOnlyCollection<BoundProjectedValue> localValues)
        {
            if (localValues.Count == 0)
                return input;

            // TODO: We should somehow leave the ValueSlot in there
            var expressions = localValues.Select(lv => AlgebrizeExpression(lv.Expression)).ToArray();

            return new AlgebraComputeNode(input, expressions);
        }

        private AlgebraRelation AlgebrizeGroupByAndAggregation(AlgebraRelation input, BoundGroupByClause groupByClause)
        {
            if (groupByClause == null)
                return input;

            return new AlgebraGroupByAndAggregation(input, groupByClause.Columns);
        }

        private AlgebraRelation AlgebrizeOrderBy(AlgebraRelation input, BoundOrderByClause orderByClause)
        {
            if (orderByClause == null)
                return input;

            var expressions = orderByClause.Columns.Select(c => c.ValueSlot).ToArray();
            return new AlgebraSortNode(input, expressions);
        }

        private AlgebraRelation AlgebrizeTop(AlgebraRelation input, int? top, bool withTies)
        {
            return top == null
                       ? input
                       : new AlgebraTopNode(input, top.Value, withTies);
        }

        private AlgebraRelation AlgebrizeCombinedQuery(BoundCombinedQuery node)
        {
            var left = AlgebrizeQuery(node.Left);
            var right = AlgebrizeQuery(node.Right);
            var combinator = GetQueryCombinator(node.Combinator);
            return new AlgebraBinaryQueryNode(left, right, combinator);
        }
    }
}