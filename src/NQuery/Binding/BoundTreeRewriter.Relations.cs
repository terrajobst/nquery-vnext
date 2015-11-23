using System;

namespace NQuery.Binding
{
    partial class BoundTreeRewriter
    {
        public virtual BoundRelation RewriteRelation(BoundRelation node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.TableRelation:
                    return RewriteTableRelation((BoundTableRelation) node);
                case BoundNodeKind.DerivedTableRelation:
                    return RewriteDerivedTableRelation((BoundDerivedTableRelation)node);
                case BoundNodeKind.JoinRelation:
                    return RewriteJoinRelation((BoundJoinRelation)node);
                case BoundNodeKind.HashMatchRelation:
                    return RewriteHashMatchRelation((BoundHashMatchRelation)node);
                case BoundNodeKind.ComputeRelation:
                    return RewriteComputeRelation((BoundComputeRelation)node);
                case BoundNodeKind.FilterRelation:
                    return RewriteFilterRelation((BoundFilterRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return RewriteGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.StreamAggregatesRelation:
                    return RewriteStreamAggregatesRelation((BoundStreamAggregatesRelation)node);
                case BoundNodeKind.ConstantRelation:
                    return RewriteConstantRelation((BoundConstantRelation)node);
                case BoundNodeKind.UnionRelation:
                    return RewriteUnionRelation((BoundUnionRelation)node);
                case BoundNodeKind.ConcatenationRelation:
                    return RewriteConcatenationRelation((BoundConcatenationRelation)node);
                case BoundNodeKind.IntersectOrExceptRelation:
                    return RewriteIntersectOrExceptRelation((BoundIntersectOrExceptRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return RewriteProjectRelation((BoundProjectRelation)node);
                case BoundNodeKind.SortRelation:
                    return RewriteSortRelation((BoundSortRelation)node);
                case BoundNodeKind.TopRelation:
                    return RewriteTopRelation((BoundTopRelation)node);
                case BoundNodeKind.AssertRelation:
                    return RewriteAssertRelation((BoundAssertRelation)node);
                case BoundNodeKind.TableSpoolPusher:
                    return RewriteTableSpoolPusher((BoundTableSpoolPusher)node);
                case BoundNodeKind.TableSpoolPopper:
                    return RewriteTableSpoolPopper((BoundTableSpoolPopper)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        protected virtual BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            return node;
        }

        protected virtual BoundRelation RewriteDerivedTableRelation(BoundDerivedTableRelation node)
        {
            return node.Update(node.TableInstance,
                               RewriteRelation(node.Relation));
        }

        protected virtual BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            return node.Update(node.JoinType,
                               RewriteRelation(node.Left),
                               RewriteRelation(node.Right),
                               RewriteExpression(node.Condition),
                               RewriteValueSlot(node.Probe),
                               RewriteExpression(node.PassthruPredicate));
        }

        protected virtual BoundRelation RewriteHashMatchRelation(BoundHashMatchRelation node)
        {
            return node.Update(node.LogicalOperator,
                               RewriteRelation(node.Build),
                               RewriteRelation(node.Probe),
                               RewriteValueSlot(node.BuildKey),
                               RewriteValueSlot(node.ProbeKey),
                               RewriteExpression(node.Remainder));
        }

        protected virtual BoundRelation RewriteComputeRelation(BoundComputeRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteComputedValues(node.DefinedValues));
        }

        protected virtual BoundRelation RewriteFilterRelation(BoundFilterRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteExpression(node.Condition));
        }

        protected virtual BoundRelation RewriteGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteComparedValues(node.Groups),
                               RewriteAggregatedValues(node.Aggregates));
        }

        protected virtual BoundRelation RewriteStreamAggregatesRelation(BoundStreamAggregatesRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteComparedValues(node.Groups),
                               RewriteAggregatedValues(node.Aggregates));
        }

        protected virtual BoundRelation RewriteConstantRelation(BoundConstantRelation node)
        {
            return node;
        }

        protected virtual BoundRelation RewriteUnionRelation(BoundUnionRelation node)
        {
            return node.Update(node.IsUnionAll,
                               RewriteRelations(node.Inputs),
                               RewriteUnifiedValues(node.DefinedValues),
                               node.Comparers);
        }

        protected virtual BoundRelation RewriteConcatenationRelation(BoundConcatenationRelation node)
        {
            return node.Update(RewriteRelations(node.Inputs),
                               RewriteUnifiedValues(node.DefinedValues));
        }

        protected virtual BoundRelation RewriteIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            return node.Update(node.IsIntersect,
                               RewriteRelation(node.Left),
                               RewriteRelation(node.Right),
                               node.Comparers);
        }

        protected virtual BoundRelation RewriteProjectRelation(BoundProjectRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteValueSlots(node.Outputs));
        }

        protected virtual BoundRelation RewriteSortRelation(BoundSortRelation node)
        {
            return node.Update(node.IsDistinct,
                               RewriteRelation(node.Input),
                               RewriteComparedValues(node.SortedValues));
        }

        protected virtual BoundRelation RewriteTopRelation(BoundTopRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               node.Limit,
                               RewriteComparedValues(node.TieEntries));
        }

        protected virtual BoundRelation RewriteAssertRelation(BoundAssertRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteExpression(node.Condition),
                               node.Message);
        }

        protected virtual BoundRelation RewriteTableSpoolPusher(BoundTableSpoolPusher node)
        {
            return node.Update(RewriteRelation(node.Input));
        }

        protected virtual BoundRelation RewriteTableSpoolPopper(BoundTableSpoolPopper node)
        {
            return node.Update(RewriteValueSlots(node.Outputs));
        }
   }
}