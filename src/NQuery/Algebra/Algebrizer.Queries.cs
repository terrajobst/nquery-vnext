using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private AlgebraNode AlgebrizeQuery(BoundQuery node)
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

        private AlgebraNode AlgebrizeSelectQuery(BoundSelectQuery node)
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

        private AlgebraNode AlgebrizeFrom(BoundTableReference boundTableReference)
        {
            if (boundTableReference == null)
                return new AlgebraConstantNode();

            return AlgebrizeTableReference(boundTableReference);
        }

        private AlgebraNode AlgebrizeFilter(AlgebraNode input, BoundExpression condition)
        {
            if (condition == null)
                return input;

            var algebrizedExpression = AlgebrizeExpression(input, condition);
            var resultInput = algebrizedExpression.Input;
            var resultCondition = algebrizedExpression.Expression;

            return new AlgebraFilterNode(resultInput, resultCondition);
        }

        private AlgebraNode AlgebrizeSelect(AlgebraNode input, IEnumerable<BoundSelectColumn> selectColumns)
        {
            var expresions = selectColumns.Select(c => c.Expression).ToArray();
            var algebrizedExpressions = AlgebrizeExpressionList(input, expresions);

            var resultInput = algebrizedExpressions.Input;
            var resultExpressions = algebrizedExpressions.Expressions;

            return new AlgebraComputeNode(resultInput, resultExpressions);
        }

        private AlgebraNode AlgebrizeTop(AlgebraNode input, int? top, bool withTies)
        {
            return top == null ? input : new AlgebraTopNode(input, top.Value, withTies);
        }

        private AlgebraNode AlgebrizeCombinedQuery(BoundCombinedQuery node)
        {
            throw new NotImplementedException("UNION/EXCEPT/INTERSECT not implemented yet");
        }

        private AlgebraNode AlgebrizeCommonTableExpressionQuery(BoundCommonTableExpressionQuery node)
        {
            // Note: we don't need to do anything with the CTEs themselves -- they will be
            //       instantiated when they are being used.
            return Algebrize(node.Query);
        }

    }
}