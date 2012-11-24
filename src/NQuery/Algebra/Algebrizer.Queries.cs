using System;
using System.Collections.Generic;
using System.Linq;

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
                case BoundNodeKind.CommonTableExpressionQuery:
                    return AlgebrizeCommonTableExpressionQuery((BoundCommonTableExpressionQuery)node);
                default:
                    throw new ArgumentException();
            }
        }

        private AlgebraRelation AlgebrizeSelectQuery(BoundSelectQuery node)
        {
            var algebrizedFrom = AlgebrizeFrom(node.FromClause);
            var algebrizedWhere = AlgebrizeFilter(algebrizedFrom, node.WhereClause);

            // TODO: Handle GROUP BY

            var algebrizedSelect = AlgebrizeSelect(algebrizedWhere, node.SelectColumns);

            // TODO: Handle HAVING
            // TODO: Handle ORDER BY

            var algebrizedTop = AlgebrizeTop(algebrizedSelect, node.Top, node.WithTies);

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

        private AlgebraRelation AlgebrizeSelect(AlgebraRelation input, IEnumerable<BoundSelectColumn> selectColumns)
        {
            var expresions = selectColumns.Select(c => AlgebrizeExpression(c.Expression)).ToArray();
            return new AlgebraComputeNode(input, expresions);
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

        private AlgebraRelation AlgebrizeCommonTableExpressionQuery(BoundCommonTableExpressionQuery node)
        {
            // Note: we don't need to do anything with the CTEs themselves -- they will be
            //       instantiated when they are being used.
            return AlgebrizeQuery(node.Query);
        }
    }
}