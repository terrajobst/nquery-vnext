using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Optimization;

internal sealed class JoinConditionValueSlotExtractor : BoundTreeRewriter
{
    protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
    {
        node = (BoundJoinRelation)base.RewriteJoinRelation(node);

        var valueSlotFactory = node.GetOutputValues().FirstOrDefault()?.Factory ?? new ValueSlotFactory();
        var leftOutputValues = node.Left.GetOutputValues().ToImmutableArray();
        var rightOutputValues = node.Right.GetOutputValues().ToImmutableArray();
        var extractor = new ConjunctionValueSlotExtractor(valueSlotFactory, leftOutputValues, rightOutputValues);
        var conjunctions = Expression.SplitConjunctions(node.Condition);

        foreach (var conjunction in conjunctions)
        {
            extractor.Process(conjunction);
        }

        var left = extractor.NewComputedValuesLeft.Count == 0
                       ? node.Left
                       : new BoundComputeRelation(node.Left, extractor.NewComputedValuesLeft);
        var right = extractor.NewComputedValuesRight.Count == 0
                        ? node.Right
                        : new BoundComputeRelation(node.Right, extractor.NewComputedValuesRight);
        var condition = extractor.NewComputedValuesLeft.Count == 0 && extractor.NewComputedValuesRight.Count == 0
                            ? node.Condition
                            : Expression.And(extractor.Conjunctions);

        return node.Update(node.JoinType, left, right, condition, node.Probe, node.PassthruPredicate);
    }

    private sealed class ConjunctionValueSlotExtractor
    {
        private readonly ValueSlotFactory _factory;
        private readonly ImmutableArray<ValueSlot> _leftOutputValues;
        private readonly ImmutableArray<ValueSlot> _rightOutputValues;

        public ConjunctionValueSlotExtractor(ValueSlotFactory valueSlotFactory, ImmutableArray<ValueSlot> leftOutputValues, ImmutableArray<ValueSlot> rightOutputValues)
        {
            _factory = valueSlotFactory;
            _leftOutputValues = leftOutputValues;
            _rightOutputValues = rightOutputValues;
        }

        public List<BoundExpression> Conjunctions { get; } = new();
        public List<BoundComputedValue> NewComputedValuesLeft { get; } = new();
        public List<BoundComputedValue> NewComputedValuesRight { get; } = new();

        public void Process(BoundExpression conjunction)
        {
            if (conjunction is not BoundBinaryExpression { OperatorKind: BinaryOperatorKind.Equal } binaryExpression)
            {
                Conjunctions.Add(conjunction);
                return;
            }

            var newLeft = ProcessSide(binaryExpression.Left);
            var newRight = ProcessSide(binaryExpression.Right);

            var updatedConjunction = binaryExpression.Update(newLeft, binaryExpression.OperatorKind, binaryExpression.Result, newRight);
            Conjunctions.Add(updatedConjunction);
        }

        private BoundExpression ProcessSide(BoundExpression boundExpression)
        {
            if (boundExpression is BoundValueSlotExpression)
                return boundExpression;

            var dependencyFinder = new ValueSlotDependencyFinder();
            dependencyFinder.VisitExpression(boundExpression);
            var valueSlots = dependencyFinder.ValueSlots;

            if (valueSlots.Count > 0)
            {
                if (valueSlots.All(_leftOutputValues.Contains))
                    return ReplaceExpression(boundExpression, NewComputedValuesLeft);

                if (valueSlots.All(_rightOutputValues.Contains))
                    return ReplaceExpression(boundExpression, NewComputedValuesRight);
            }

            return boundExpression;
        }

        private BoundExpression ReplaceExpression(BoundExpression boundExpression, List<BoundComputedValue> newComputedValues)
        {
            var valueSlot = _factory.CreateTemporary(boundExpression.Type);
            var computedValue = new BoundComputedValue(boundExpression, valueSlot);
            newComputedValues.Add(computedValue);
            return Expression.Value(valueSlot);
        }
    }
}
