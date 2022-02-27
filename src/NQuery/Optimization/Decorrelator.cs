using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class Decorrelator : BoundTreeRewriter
    {
        private bool ConjunctionHasOuterReference(HashSet<ValueSlot> definedValueSet, BoundExpression conjunction)
        {
            var valueSlotFinder = new ValueSlotDependencyFinder();
            valueSlotFinder.VisitExpression(conjunction);
            return !valueSlotFinder.ValueSlots.All(definedValueSet.Contains);
        }

        private static bool SemiJoinDoesNotDependOn(BoundJoinType joinType, BoundExpression conjunction, HashSet<ValueSlot> rightDefinedValues)
        {
            if (joinType == BoundJoinType.LeftSemi ||
                joinType == BoundJoinType.LeftAntiSemi)
            {
                return !Expression.DependsOnAny(conjunction, rightDefinedValues);
            }

            return true;
        }

        protected override BoundRelation RewriteSortRelation(BoundSortRelation node)
        {
            var newInput = RewriteRelation(node.Input);

            if (newInput is BoundFilterRelation filter)
            {
                node = node.Update(node.IsDistinct, filter.Input, node.SortedValues);
                return filter.WithInput(node);
            }
            else
            {
                return node.WithInput(newInput);
            }
        }

        protected override BoundRelation RewriteComputeRelation(BoundComputeRelation node)
        {
            var newInput = RewriteRelation(node.Input);

            if (newInput is BoundFilterRelation filter)
            {
                node = node.Update(filter.Input, node.DefinedValues);
                return filter.WithInput(node);
            }
            else
            {
                return node.WithInput(newInput);
            }
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            // First, let's rewrite our inputs

            node = (BoundJoinRelation) base.RewriteJoinRelation(node);

            // Get defined values of left and right

            var leftDefinedValues = new HashSet<ValueSlot>(node.Left.GetDefinedValues());
            var rightDefinedValues = new HashSet<ValueSlot>(node.Right.GetDefinedValues());

            var extractedConjunctions = new List<BoundExpression>();

            // Try to pull up conjunctions that contain outer references from a left sided filter and combine
            // them with the join predicate.
            //
            // NOTE: This is only possible if the join is not a LEFT OUTER or FULL OUTER JOIN since this
            // operation would change the join's semantic.

            if (node.JoinType != BoundJoinType.LeftOuter &&
                node.JoinType != BoundJoinType.FullOuter)
            {
                if (node.Left is BoundFilterRelation leftAsFilter)
                {
                    var anythingExtracted = false;
                    var remainingConjunctions = new List<BoundExpression>();

                    foreach (var conjunction in Expression.SplitConjunctions(leftAsFilter.Condition))
                    {
                        if (!ConjunctionHasOuterReference(leftDefinedValues, conjunction))
                        {
                            remainingConjunctions.Add(conjunction);
                        }
                        else
                        {
                            anythingExtracted = true;
                            extractedConjunctions.Add(conjunction);
                        }
                    }

                    if (!anythingExtracted)
                    {
                        // We haven't extracted any conjunctions.
                        //
                        // In order to avoid creating new node, we just do nothing.
                    }
                    else
                    {
                        var newCondition = Expression.And(remainingConjunctions);
                        if (newCondition is null)
                        {
                            node = node.WithLeft(leftAsFilter.Input);
                        }
                        else
                        {
                            leftAsFilter = leftAsFilter.WithCondition(newCondition);
                            node = node.WithLeft(leftAsFilter);
                        }
                    }
                }
            }

            // Try to pull up conjunctions that contain outer references from a right sided filter and combine
            // them with the join predicate.
            //
            // NOTE: This is only possible if the join is not a RIGHT OUTER or FULL OUTER JOIN since this
            // operation would change the join's semantic.

            if (node.JoinType != BoundJoinType.RightOuter &&
                node.JoinType != BoundJoinType.FullOuter)
            {
                if (node.Right is BoundFilterRelation rightAsFilter)
                {
                    var anythingExtracted = false;
                    var remainingConjunctions = new List<BoundExpression>();

                    foreach (var conjunction in Expression.SplitConjunctions(rightAsFilter.Condition))
                    {
                        if (!ConjunctionHasOuterReference(rightDefinedValues, conjunction))
                        {
                            remainingConjunctions.Add(conjunction);
                        }
                        else
                        {
                            anythingExtracted = true;
                            extractedConjunctions.Add(conjunction);
                        }
                    }

                    if (!anythingExtracted)
                    {
                        // We haven't extracted any conjunctions.
                        //
                        // In order to avoid creating new node, we just do nothing.
                    }
                    else
                    {
                        var newCondition = Expression.And(remainingConjunctions);
                        if (newCondition is null)
                        {
                            node = node.WithRight(rightAsFilter.Input);
                        }
                        else
                        {
                            rightAsFilter = rightAsFilter.WithCondition(newCondition);
                            node = node.WithRight(rightAsFilter);
                        }
                    }
                }
            }

            // If we found any conjunctions that could be pulled up, merge them with the join predicate.

            if (extractedConjunctions.Any())
            {
                var newCondition = Expression.And(new[] { node.Condition }.Concat(extractedConjunctions));
                node = node.WithCondition(newCondition);
            }

            // Now we try to extract conjunctions that contain outer references from the join
            // predicate itself.
            //
            // NOTE: This is only possible if the node is not an OUTER JOIN. If the node is a
            // SEMI JOIN the operation is only legal if the conjunction does not reference any
            // columns from the side that is used as filter criteria (i.e. for LSJ this is the
            // right side, for RSJ this is the left side).

            if (node.JoinType != BoundJoinType.LeftOuter &&
                node.JoinType != BoundJoinType.RightOuter &&
                node.JoinType != BoundJoinType.FullOuter &&
                node.Condition is not null)
            {
                var conjunctionsAboveJoin = new List<BoundExpression>();
                var remainingConjunctions = new List<BoundExpression>();
                var definedValues = new HashSet<ValueSlot>(leftDefinedValues.Concat(rightDefinedValues));

                foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
                {
                    if (ConjunctionHasOuterReference(definedValues, conjunction) && SemiJoinDoesNotDependOn(node.JoinType, conjunction, rightDefinedValues))
                    {
                        conjunctionsAboveJoin.Add(conjunction);
                    }
                    else
                    {
                        remainingConjunctions.Add(conjunction);
                    }
                }

                if (conjunctionsAboveJoin.Any())
                {
                    var newCondition = Expression.And(remainingConjunctions);
                    node = node.WithCondition(newCondition);

                    var filterCondition = Expression.And(conjunctionsAboveJoin);
                    var filter = new BoundFilterRelation(node, filterCondition);
                    return filter;
                }
            }

            return node;
        }
    }
}