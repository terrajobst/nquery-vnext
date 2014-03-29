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
                case BoundNodeKind.ComputeRelation:
                    return RewriteComputeRelation((BoundComputeRelation)node);
                case BoundNodeKind.FilterRelation:
                    return RewriteFilterRelation((BoundFilterRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return RewriteGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.ConstantRelation:
                    return RewriteConstantRelation((BoundConstantRelation)node);
                case BoundNodeKind.CombinedRelation:
                    return RewriteCombinedRelation((BoundCombinedRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return RewriteProjectRelation((BoundProjectRelation)node);
                case BoundNodeKind.SortRelation:
                    return RewriteSortRelation((BoundSortRelation)node);
                case BoundNodeKind.TopRelation:
                    return RewriteTopRelation((BoundTopRelation)node);
                default:
                    throw new ArgumentOutOfRangeException();
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
                               RewriteExpression(node.Condition));
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
                               RewriteValueSlots(node.Groups),
                               RewriteAggregatedValues(node.Aggregates));
        }

        protected virtual BoundRelation RewriteConstantRelation(BoundConstantRelation node)
        {
            return node;
        }

        protected virtual BoundRelation RewriteCombinedRelation(BoundCombinedRelation node)
        {
            return node.Update(node.Combinator,
                               RewriteRelation(node.Left),
                               RewriteRelation(node.Right),
                               RewriteValueSlots(node.Outputs));
        }

        protected virtual BoundRelation RewriteProjectRelation(BoundProjectRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteValueSlots(node.Outputs));
        }

        protected virtual BoundRelation RewriteSortRelation(BoundSortRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               RewriteSortedValues(node.SortedValues));
        }

        protected virtual BoundRelation RewriteTopRelation(BoundTopRelation node)
        {
            return node.Update(RewriteRelation(node.Input),
                               node.Limit,
                               RewriteSortedValues(node.TieEntries));
        }
   }
}