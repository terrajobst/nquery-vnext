using System;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private static AlgebraJoinKind GetJoinKind(BoundJoinType joinType)
        {
            switch (joinType)
            {
                case BoundJoinType.Inner:
                    return AlgebraJoinKind.Inner;
                case BoundJoinType.FullOuter:
                    return AlgebraJoinKind.FullOuter;
                case BoundJoinType.LeftOuter:
                    return AlgebraJoinKind.LeftOuter;
                case BoundJoinType.RightOuter:
                    return AlgebraJoinKind.RightOuter;
                default:
                    throw new ArgumentOutOfRangeException("joinType");
            }
        }

        private AlgebraRelation AlgebrizeTableReference(BoundTableReference node)
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

        private AlgebraRelation AlgebrizeNamedTableReference(BoundNamedTableReference node)
        {
            // TODO: If expressions.TableInstance refers to CTE, we need to instantiate the CTE here.

            var cte = node.TableInstance.Table as CommonTableExpressionSymbol;
            return cte == null
                       ? new AlgebraTableNode(node.TableInstance)
                       : AlgebrizeQuery(cte.Query);
        }

        private AlgebraRelation AlgebrizeDerivedTableReference(BoundDerivedTableReference node)
        {
            return AlgebrizeQuery(node.Query);
        }

        private AlgebraRelation AlgebrizeJoinedTableReference(BoundJoinedTableReference node)
        {
            return AlgebrizeJoin(node.JoinType, node.Left, node.Right, node.Condition);
        }

        private AlgebraRelation AlgebrizeJoin(BoundJoinType joinType, BoundTableReference left, BoundTableReference right, BoundExpression condition)
        {
            var physicalJoin = GetJoinKind(joinType);
            var algebrizedLeft = AlgebrizeTableReference(left);
            var algebrizedRight = AlgebrizeTableReference(right);
            var algebrizedCondition = condition == null
                                          ? null
                                          : AlgebrizeExpression(condition);
            return new AlgebraJoinNode(physicalJoin, algebrizedLeft, algebrizedRight, algebrizedCondition);
        }
    }
}