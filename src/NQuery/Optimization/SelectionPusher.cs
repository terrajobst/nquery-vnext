using System;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class SelectionPusher : BoundTreeRewriter
    {
        protected override BoundRelation RewriteFilterRelation(BoundFilterRelation node)
        {
            switch (node.Input.Kind)
            {
                case BoundNodeKind.ProjectRelation:
                    return PushOverProject(node, (BoundProjectRelation)node.Input);

                case BoundNodeKind.SortRelation:
                    return PushOverSort(node, (BoundSortRelation)node.Input);

                case BoundNodeKind.TopRelation:
                    return PushOverTop(node, (BoundTopRelation)node.Input);

                case BoundNodeKind.FilterRelation:
                    return MergeWithFilter(node, (BoundFilterRelation)node.Input);

                case BoundNodeKind.ComputeRelation:
                    return PushOverCompute(node, (BoundComputeRelation)node.Input);

                case BoundNodeKind.GroupByAndAggregationRelation:
                    return PushOverGroupByAndAggregation(node, (BoundGroupByAndAggregationRelation)node.Input);

                case BoundNodeKind.JoinRelation:
                    return MergeWithOrPushOverJoin(node, (BoundJoinRelation)node.Input);

                //case BoundNodeKind.CombinedRelation:
                //    return PushOverCombined(node, (BoundCombinedRelation)node.Input);
            }

            return base.RewriteFilterRelation(node);
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            if (node.Condition != null && node.Probe == null)
            {
                BoundExpression pushedLeft = null;
                BoundExpression pushedRight = null;
                BoundExpression remainder = null;

                foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
                {
                    if (AllowsLeftPushDown(node.JoinType) && !conjunction.DependsOnAny(node.Right.GetOutputValues()))
                    {
                        pushedLeft = Expression.And(pushedLeft, conjunction);
                    }
                    else if (AllowsRightPushDown(node.JoinType) && !conjunction.DependsOnAny(node.Left.GetOutputValues()))
                    {
                        pushedRight = Expression.And(pushedRight, conjunction);
                    }
                    else
                    {
                        remainder = Expression.And(remainder, conjunction);
                    }
                }

                var newLeft = pushedLeft == null
                    ? node.Left
                    : new BoundFilterRelation(node.Left, pushedLeft);

                var newRight = pushedRight == null
                    ? node.Right
                    : new BoundFilterRelation(node.Right, pushedRight);

                var newNode = node.Update(node.JoinType,
                                          RewriteRelation(newLeft),
                                          RewriteRelation(newRight),
                                          remainder,
                                          RewriteValueSlot(node.Probe),
                                          RewriteExpression(node.PassthruPredicate));
                return newNode;
            }

            return base.RewriteJoinRelation(node);
        }

        private static bool AllowsLeftPushDown(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.RightOuter ||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.LeftAntiSemi;
        }

        private static bool AllowsRightPushDown(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.LeftOuter ||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.LeftAntiSemi;
        }

        private static bool AllowsLeftPushOver(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.LeftOuter||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.LeftAntiSemi;
        }

        private static bool AllowsRightPushOver(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.RightOuter||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.LeftAntiSemi;
        }

        private static bool AllowsMerge(BoundJoinType type)
        {
            return type == BoundJoinType.Inner;
        }

        private BoundRelation PushOverProject(BoundFilterRelation node, BoundProjectRelation input)
        {
            var newFilter = RewriteRelation(node.WithInput(input.Input));
            var newInput = input.Update(newFilter, input.Outputs);
            return newInput;
        }

        private BoundRelation PushOverSort(BoundFilterRelation node, BoundSortRelation input)
        {
            var newFilter = RewriteRelation(node.WithInput(input.Input));
            var newInput = input.Update(input.IsDistinct, newFilter, input.SortedValues);
            return newInput;
        }

        private BoundRelation PushOverTop(BoundFilterRelation node, BoundTopRelation input)
        {
            var newFilter = RewriteRelation(node.WithInput(input.Input));
            var newInput = input.Update(newFilter, input.Limit, input.TieEntries);
            return newInput;
        }

        private BoundRelation MergeWithFilter(BoundFilterRelation node, BoundFilterRelation input)
        {
            var mergedCondition = Expression.And(input.Condition, node.Condition);
            var newInput = RewriteRelation(input.WithCondition(mergedCondition));
            return newInput;
        }

        private BoundRelation PushOverCompute(BoundFilterRelation node, BoundComputeRelation input)
        {
            BoundExpression remainder = null;
            BoundExpression pushed = null;

            foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
            {
                if (conjunction.DependsOnAny(input.GetDefinedValues()))
                {
                    remainder = Expression.And(remainder, conjunction);
                }
                else
                {
                    pushed = Expression.And(pushed, conjunction);
                }
            }

            var newInputInput = pushed == null
                ? input.Input
                : new BoundFilterRelation(input.Input, pushed);

            var newInput = RewriteRelation(input.Update(newInputInput, input.DefinedValues));

            var newNode = remainder == null
                ? newInput
                : new BoundFilterRelation(newInput, remainder);

            return newNode;
        }

        private BoundRelation PushOverGroupByAndAggregation(BoundFilterRelation node, BoundGroupByAndAggregationRelation input)
        {
            // TODO: This may not be that easy.
            //
            // For example this condition can be pushed:
            //
            //      GroupCol = Value
            //
            //  while this can't:
            //
            //      GroupCol = Value OR Func(const) = const
            //
            // Formally, a predicate can be pushed over an aggregate if and only if all disjuncts of the predicate's
            // CNF do reference at least one grouping column.

            BoundExpression remainder = null;
            BoundExpression pushed = null;

            foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
            {
                if (conjunction.DependsOnAny(input.GetDefinedValues()))
                {
                    remainder = Expression.And(remainder, conjunction);
                }
                else
                {
                    pushed = Expression.And(pushed, conjunction);
                }
            }

            var newInputInput = pushed == null
                ? input.Input
                : new BoundFilterRelation(input.Input, pushed);

            var newInput = RewriteRelation(input.Update(newInputInput, input.Groups, input.Aggregates));

            var newNode = remainder == null
                ? newInput
                : new BoundFilterRelation(newInput, remainder);

            return newNode;
        }

        private BoundRelation MergeWithOrPushOverJoin(BoundFilterRelation node, BoundJoinRelation input)
        {
            // TODO: Right now, we're not pushing over a join if it has any probing.
            //
            //       That might be too restrictive. It should be OK to push over
            //       a join to the side that isn't affecting the probe column.
            //
            //       In other words:
            //
            //       * pushing to the left is OK if it's a left (anti) semi join
            //       * pushing to the right is OK if it's a right (anti) semi join

            if (input.Probe != null)
                return node.WithInput(RewriteRelation(input));

            if (AllowsMerge(input.JoinType))
            {
                var newCondition = Expression.And(input.Condition, node.Condition);
                var newInput = input.WithCondition(newCondition);
                return RewriteRelation(newInput);
            }
            else
            {
                BoundExpression pushedLeft = null;
                BoundExpression pushedRight = null;
                BoundExpression remainder = null;

                foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
                {
                    if (AllowsLeftPushOver(input.JoinType) && !conjunction.DependsOnAny(input.Right.GetOutputValues()))
                    {
                        pushedLeft = Expression.And(pushedLeft, conjunction);
                    }
                    else if (AllowsRightPushOver(input.JoinType) && !conjunction.DependsOnAny(input.Left.GetOutputValues()))
                    {
                        pushedRight = Expression.And(pushedRight, conjunction);
                    }
                    else
                    {
                        remainder = Expression.And(remainder, conjunction);
                    }
                }

                var newLeft = pushedLeft == null
                    ? input.Left
                    : new BoundFilterRelation(input.Left, pushedLeft);

                var newRight = pushedRight == null
                    ? input.Right
                    : new BoundFilterRelation(input.Right, pushedRight);

                var newInput = input.Update(input.JoinType,
                                            RewriteRelation(newLeft),
                                            RewriteRelation(newRight),
                                            input.Condition,
                                            RewriteValueSlot(input.Probe),
                                            RewriteExpression(input.PassthruPredicate));

                var newNode = remainder == null
                    ? (BoundRelation)newInput
                    : node.Update(newInput, remainder);

                return newNode;
            }
        }
    }
}