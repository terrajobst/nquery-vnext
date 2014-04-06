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
            if (node.Condition != null)
            {
                BoundExpression pushedLeft = null;
                BoundExpression pushedRight = null;
                BoundExpression remainder = null;

                foreach (var conjunction in Condition.SplitConjunctions(node.Condition))
                {
                    if (AllowsLeftPushDown(node.JoinType) && !conjunction.DependsOnAny(node.Right.GetOutputValues()))
                    {
                        pushedLeft = Condition.And(pushedLeft, conjunction);
                    }
                    else if (AllowsRightPushDown(node.JoinType) && !conjunction.DependsOnAny(node.Left.GetOutputValues()))
                    {
                        pushedRight = Condition.And(pushedRight, conjunction);
                    }
                    else
                    {
                        remainder = Condition.And(remainder, conjunction);
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
                                          remainder);
                return newNode;
            }

            return base.RewriteJoinRelation(node);
        }

        private static bool AllowsLeftPushDown(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.RightOuter /* ||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.RightSemi ||
                   type == BoundJoinType.LeftAntiSemi ||
                   type == BoundJoinType.RightAntiSemi */;
        }

        private static bool AllowsRightPushDown(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.LeftOuter /* ||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.RightSemi ||
                   type == BoundJoinType.LeftAntiSemi ||
                   type == BoundJoinType.RightAntiSemi*/;
        }

        private static bool AllowsLeftPushOver(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.LeftOuter /*||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.RightSemi ||
                   type == BoundJoinType.LeftAntiSemi ||
                   type == BoundJoinType.RightAntiSemi*/;
        }

        private static bool AllowsRightPushOver(BoundJoinType type)
        {
            return type == BoundJoinType.Inner ||
                   type == BoundJoinType.RightOuter /*||
                   type == BoundJoinType.LeftSemi ||
                   type == BoundJoinType.RightSemi ||
                   type == BoundJoinType.LeftAntiSemi ||
                   type == BoundJoinType.RightAntiSemi*/;
        }

        private static bool AllowsMerge(BoundJoinType type)
        {
            return type == BoundJoinType.Inner;
        }

        private BoundRelation PushOverProject(BoundFilterRelation node, BoundProjectRelation input)
        {
            var newFilter = RewriteRelation(node.Update(input.Input, node.Condition));
            var newInput = input.Update(newFilter, input.Outputs);
            return newInput;
        }

        private BoundRelation PushOverSort(BoundFilterRelation node, BoundSortRelation input)
        {
            var newFilter = RewriteRelation(node.Update(input.Input, node.Condition));
            var newInput = input.Update(input.IsDistinct, newFilter, input.SortedValues);
            return newInput;
        }

        private BoundRelation PushOverTop(BoundFilterRelation node, BoundTopRelation input)
        {
            var newFilter = RewriteRelation(node.Update(input.Input, node.Condition));
            var newInput = input.Update(newFilter, input.Limit, input.TieEntries);
            return newInput;
        }

        private BoundRelation MergeWithFilter(BoundFilterRelation node, BoundFilterRelation input)
        {
            var mergedCondition = Condition.And(input.Condition, node.Condition);
            var newInput = RewriteRelation(input.Update(input.Input, mergedCondition));
            return newInput;
        }
    
        private BoundRelation PushOverCompute(BoundFilterRelation node, BoundComputeRelation input)
        {
            BoundExpression remainder = null;
            BoundExpression pushed = null;

            foreach (var conjunction in Condition.SplitConjunctions(node.Condition))
            {
                if (conjunction.DependsOnAny(input.GetDefinedValues()))
                {
                    remainder = Condition.And(remainder, conjunction);
                }
                else
                {
                    pushed = Condition.And(pushed, conjunction);
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

            foreach (var conjunction in Condition.SplitConjunctions(node.Condition))
            {
                if (conjunction.DependsOnAny(input.GetDefinedValues()))
                {
                    remainder = Condition.And(remainder, conjunction);
                }
                else
                {
                    pushed = Condition.And(pushed, conjunction);
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
            if (AllowsMerge(input.JoinType))
            {
                var newCondition = Condition.And(input.Condition, node.Condition);
                var newInput = input.Update(input.JoinType, input.Left, input.Right, newCondition);
                return RewriteRelation(newInput);
            }
            else
            {
                BoundExpression pushedLeft = null;
                BoundExpression pushedRight = null;
                BoundExpression remainder = null;

                foreach (var conjunction in Condition.SplitConjunctions(node.Condition))
                {
                    if (AllowsLeftPushOver(input.JoinType) && !conjunction.DependsOnAny(input.Right.GetOutputValues()))
                    {
                        pushedLeft = Condition.And(pushedLeft, conjunction);
                    }
                    else if (AllowsRightPushOver(input.JoinType) && !conjunction.DependsOnAny(input.Left.GetOutputValues()))
                    {
                        pushedRight = Condition.And(pushedRight, conjunction);
                    }
                    else
                    {
                        remainder = Condition.And(remainder, conjunction);
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
                                            input.Condition);

                var newNode = remainder == null
                    ? (BoundRelation)newInput
                    : node.Update(newInput, remainder);

                return newNode;
            }
        }
    }
}