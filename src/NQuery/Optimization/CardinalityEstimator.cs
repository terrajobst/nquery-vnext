using System;
using System.Collections;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal static class CardinalityEstimator
    {
        public static CardinalityEstimate Estimate(BoundRelation relation)
        {
            switch (relation.Kind)
            {
                case BoundNodeKind.TableRelation:
                    return EstimateTableRelation((BoundTableRelation)relation);
                case BoundNodeKind.DerivedTableRelation:
                    return EstimateDerivedTableRelation((BoundDerivedTableRelation)relation);
                case BoundNodeKind.JoinRelation:
                    return EstimateJoinRelation((BoundJoinRelation)relation);
                case BoundNodeKind.HashMatchRelation:
                    return EstimateHashMatchRelation((BoundHashMatchRelation)relation);
                case BoundNodeKind.ComputeRelation:
                    return EstimateComputeRelation((BoundComputeRelation)relation);
                case BoundNodeKind.FilterRelation:
                    return EstimateFilterRelation((BoundFilterRelation)relation);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return EstimateGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)relation);
                case BoundNodeKind.StreamAggregatesRelation:
                    return EstimateStreamAggregatesRelation((BoundStreamAggregatesRelation)relation);
                case BoundNodeKind.ConstantRelation:
                    return EstimateConstantRelation((BoundConstantRelation)relation);
                case BoundNodeKind.UnionRelation:
                    return EstimateUnionRelation((BoundUnionRelation)relation);
                case BoundNodeKind.ConcatenationRelation:
                    return EstimateConcatenationRelation((BoundConcatenationRelation)relation);
                case BoundNodeKind.IntersectOrExceptRelation:
                    return EstimateIntersectOrExceptRelation((BoundIntersectOrExceptRelation)relation);
                case BoundNodeKind.ProjectRelation:
                    return EstimateProjectRelation((BoundProjectRelation)relation);
                case BoundNodeKind.SortRelation:
                    return EstimateSortRelation((BoundSortRelation)relation);
                case BoundNodeKind.TopRelation:
                    return EstimateTopRelation((BoundTopRelation)relation);
                case BoundNodeKind.AssertRelation:
                    return EstimateAssertRelation((BoundAssertRelation)relation);
                case BoundNodeKind.TableSpoolPusher:
                    return EstimateTableSpoolPusher((BoundTableSpoolPusher)relation);
                case BoundNodeKind.TableSpoolPopper:
                    return EstimateTableSpoolPopper((BoundTableSpoolPopper)relation);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static CardinalityEstimate EstimateTableRelation(BoundTableRelation relation)
        {
            var tableSymbol = relation.TableInstance.Table as SchemaTableSymbol;
            if (tableSymbol == null)
                return CardinalityEstimate.Unknown;

            var collection = tableSymbol.Definition.GetRows() as ICollection;
            if (collection == null)
                return CardinalityEstimate.Unknown;

            return new CardinalityEstimate(0, collection.Count);
        }

        private static CardinalityEstimate EstimateDerivedTableRelation(BoundDerivedTableRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateJoinRelation(BoundJoinRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateHashMatchRelation(BoundHashMatchRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateComputeRelation(BoundComputeRelation relation)
        {
            return Estimate(relation.Input);
        }

        private static CardinalityEstimate EstimateFilterRelation(BoundFilterRelation relation)
        {
            var input = Estimate(relation.Input);
            return new CardinalityEstimate(0, input.Maximum);
        }

        private static CardinalityEstimate EstimateGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateStreamAggregatesRelation(BoundStreamAggregatesRelation relation)
        {
            if (!relation.Groups.Any())
                return CardinalityEstimate.SingleRow;

            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateConstantRelation(BoundConstantRelation relation)
        {
            return CardinalityEstimate.SingleRow;
        }

        private static CardinalityEstimate EstimateUnionRelation(BoundUnionRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateConcatenationRelation(BoundConcatenationRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateIntersectOrExceptRelation(BoundIntersectOrExceptRelation relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateProjectRelation(BoundProjectRelation relation)
        {
            return Estimate(relation.Input);
        }

        private static CardinalityEstimate EstimateSortRelation(BoundSortRelation relation)
        {
            return Estimate(relation.Input);
        }

        private static CardinalityEstimate EstimateTopRelation(BoundTopRelation relation)
        {
            var input = Estimate(relation.Input);
            return relation.TieEntries.Any()
                ? input
                : new CardinalityEstimate(input.Minimum, relation.Limit);
        }

        private static CardinalityEstimate EstimateAssertRelation(BoundAssertRelation relation)
        {
            // TODO: If we knew more about the asser (such as MAX(1) or MAX(100)) we'd be able to give better estimates.
            return Estimate(relation.Input);
        }

        private static CardinalityEstimate EstimateTableSpoolPusher(BoundTableSpoolPusher relation)
        {
            return CardinalityEstimate.Unknown;
        }

        private static CardinalityEstimate EstimateTableSpoolPopper(BoundTableSpoolPopper relation)
        {
            return CardinalityEstimate.Unknown;
        }
    }
}