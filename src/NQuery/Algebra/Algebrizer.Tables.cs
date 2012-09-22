using System;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private AlgebraNode AlgebrizeTableReference(BoundTableReference node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.NamedTableReference:
                    return AlgebrizeNamedTableReference((BoundNamedTableReference)node);
                case BoundNodeKind.DerivedTableReference:
                    return AlgebrizeDerivedTableReference((BoundDerivedTableReference)node);
                case BoundNodeKind.JoinedTableReference:
                    return AlgebrizeJoinedTableReference((BoundJoinedTableReference)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AlgebraNode AlgebrizeNamedTableReference(BoundNamedTableReference node)
        {
            // TODO: If expressions.TableInstance refers to CTE, we need to instantiate the CTE here.

            return new AlgebraTableNode(node.TableInstance);
        }

        private AlgebraNode AlgebrizeDerivedTableReference(BoundDerivedTableReference node)
        {
            return Algebrize(node.Query);
        }

        private AlgebraNode AlgebrizeJoinedTableReference(BoundJoinedTableReference node)
        {
            return node.JoinType == BoundJoinType.FullOuterJoin
                       ? AlgebrizeFullOuterJoin(node)
                       : AlgebrizeJoin(node.JoinType, node.Left, node.Right, node.Condition);
        }

        private AlgebraNode AlgebrizeJoin(BoundJoinType joinType, BoundTableReference left, BoundTableReference right, BoundExpression condition)
        {
            var physicalJoin = joinType == BoundJoinType.InnerJoin ? AlgebraJoinKind.Inner : AlgebraJoinKind.LeftOuter;
            var physicalLeft = joinType == BoundJoinType.RightOuterJoin ? right : left;
            var physicalRight = joinType == BoundJoinType.RightOuterJoin ? left : right;

            var algebrizedLeft = AlgebrizeTableReference(physicalLeft);
            var algebrizedRight = AlgebrizeTableReference(physicalRight);

            if (condition == null)
                return new AlgebraJoinNode(physicalJoin, algebrizedLeft, algebrizedRight, null, null);

            var algebrizedCondition = AlgebrizeExpression(algebrizedRight, condition);
            return new AlgebraJoinNode(physicalJoin, algebrizedLeft, algebrizedCondition.Input, null, algebrizedCondition.Expression);
        }

        private AlgebraNode AlgebrizeFullOuterJoin(BoundJoinedTableReference node)
        {
            // Conceptually, full outer joins can be replaced by appending a LEFT OUTER
            // and a RIGHT OUTER. Of course, one has to be careful not to emit the rows
            // twice (a LEFT OUTER/RIGHT OUTER share the rows where match can be found.
            //
            // We substruct suppresse the common rows for the RIGHT OUTER JOIN by using
            // a LEFT ANTI SEMI JOIN that only returns where not match can be found.
            //
            // In more concrete terms, this is how the output will look like:
            //
            // CONCAT
            //      LEFT OUTER JOIN (predicate)
            //          left
            //          right
            //      COMPUTE (left := null)
            //          LEFT ANTI SEMI JOIN
            //              right
            //              top 1
            //                  FILTER (predicate)
            //                      left

            var leftOuter = AlgebrizeJoin(BoundJoinType.LeftOuterJoin, node.Left, node.Right, node.Condition);

            var left = AlgebrizeTableReference(node.Left);
            var algebrizedCondition = AlgebrizeExpression(left, node.Condition);
            var filter = new AlgebraFilterNode(algebrizedCondition.Input, algebrizedCondition.Expression);
            var top1 = new AlgebraTopNode(filter, 1, false);
            var right = AlgebrizeTableReference(node.Right);
            var leftAntiSemiJoin = new AlgebraJoinNode(AlgebraJoinKind.LeftAntiSemiJoin, right, top1, null, null);

            // TODO: We need a Compute Node that assigns null to all expressions in LEFT.

            return new AlgebraConcatNode(leftOuter, leftAntiSemiJoin);
        }

    }
}