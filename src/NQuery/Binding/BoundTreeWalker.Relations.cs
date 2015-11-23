using System;

namespace NQuery.Binding
{
    partial class BoundTreeWalker
    {
        public virtual void VisitRelation(BoundRelation node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.TableRelation:
                    VisitTableRelation((BoundTableRelation)node);
                    break;
                case BoundNodeKind.DerivedTableRelation:
                    VisitDerivedTableRelation((BoundDerivedTableRelation)node);
                    break;
                case BoundNodeKind.JoinRelation:
                    VisitJoinRelation((BoundJoinRelation)node);
                    break;
                case BoundNodeKind.HashMatchRelation:
                    VisitHashMatchRelation((BoundHashMatchRelation)node);
                    break;
                case BoundNodeKind.ComputeRelation:
                    VisitComputeRelation((BoundComputeRelation)node);
                    break;
                case BoundNodeKind.FilterRelation:
                    VisitFilterRelation((BoundFilterRelation)node);
                    break;
                case BoundNodeKind.GroupByAndAggregationRelation:
                    VisitGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)node);
                    break;
                case BoundNodeKind.StreamAggregatesRelation:
                    VisitStreamAggregatesRelation((BoundStreamAggregatesRelation)node);
                    break;
                case BoundNodeKind.ConstantRelation:
                    VisitConstantRelation((BoundConstantRelation)node);
                    break;
                case BoundNodeKind.UnionRelation:
                    VisitUnionRelation((BoundUnionRelation)node);
                    break;
                case BoundNodeKind.ConcatenationRelation:
                    VisitConcatenationRelation((BoundConcatenationRelation)node);
                    break;
                case BoundNodeKind.IntersectOrExceptRelation:
                    VisitIntersectOrExceptRelation((BoundIntersectOrExceptRelation)node);
                    break;
                case BoundNodeKind.ProjectRelation:
                    VisitProjectRelation((BoundProjectRelation)node);
                    break;
                case BoundNodeKind.SortRelation:
                    VisitSortRelation((BoundSortRelation)node);
                    break;
                case BoundNodeKind.TopRelation:
                    VisitTopRelation((BoundTopRelation)node);
                    break;
                case BoundNodeKind.AssertRelation:
                    VisitAssertRelation((BoundAssertRelation)node);
                    break;
                case BoundNodeKind.TableSpoolPusher:
                    VisitTableSpoolPusher((BoundTableSpoolPusher)node);
                    break;
                case BoundNodeKind.TableSpoolPopper:
                    VisitTableSpoolPopper((BoundTableSpoolPopper)node);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        protected virtual void VisitTableRelation(BoundTableRelation node)
        {
        }

        protected virtual void VisitDerivedTableRelation(BoundDerivedTableRelation node)
        {
            VisitRelation(node.Relation);
        }

        protected virtual void VisitJoinRelation(BoundJoinRelation node)
        {
            VisitRelation(node.Left);
            VisitRelation(node.Right);

            if (node.Condition != null)
                VisitExpression(node.Condition);

            if (node.PassthruPredicate != null)
                VisitExpression(node.PassthruPredicate);
        }

        protected virtual void VisitHashMatchRelation(BoundHashMatchRelation node)
        {
            VisitRelation(node.Build);
            VisitRelation(node.Probe);

            if (node.Remainder != null)
                VisitExpression(node.Remainder);
        }

        protected virtual void VisitComputeRelation(BoundComputeRelation node)
        {
            VisitRelation(node.Input);

            foreach (var computedValue in node.DefinedValues)
                VisitExpression(computedValue.Expression);
        }

        protected virtual void VisitFilterRelation(BoundFilterRelation node)
        {
            VisitRelation(node.Input);
            VisitExpression(node.Condition);
        }

        protected virtual void VisitGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitStreamAggregatesRelation(BoundStreamAggregatesRelation node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitConstantRelation(BoundConstantRelation node)
        {
        }

        protected virtual void VisitUnionRelation(BoundUnionRelation node)
        {
            foreach (var input in node.Inputs)
                VisitRelation(input);
        }

        protected virtual void VisitConcatenationRelation(BoundConcatenationRelation node)
        {
            foreach (var input in node.Inputs)
                VisitRelation(input);
        }

        protected virtual void VisitIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            VisitRelation(node.Left);
            VisitRelation(node.Right);
        }

        protected virtual void VisitProjectRelation(BoundProjectRelation node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitSortRelation(BoundSortRelation node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitTopRelation(BoundTopRelation node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitAssertRelation(BoundAssertRelation node)
        {
            VisitRelation(node.Input);
            VisitExpression(node.Condition);
        }

        protected virtual void VisitTableSpoolPusher(BoundTableSpoolPusher node)
        {
            VisitRelation(node.Input);
        }

        protected virtual void VisitTableSpoolPopper(BoundTableSpoolPopper node)
        {
        }
    }
}