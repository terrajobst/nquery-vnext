using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundCombinedRelation : BoundRelation
    {
        private readonly BoundQueryCombinator _combinator;
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;
        private readonly ImmutableArray<ValueSlot> _outputs;

        public BoundCombinedRelation(BoundQueryCombinator combinator, BoundRelation left, BoundRelation right, IEnumerable<ValueSlot> outputs)
        {
            _combinator = combinator;
            _left = left;
            _right = right;
            _outputs = outputs.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CombinedRelation; }
        }

        public BoundQueryCombinator Combinator
        {
            get { return _combinator; }
        }

        public BoundRelation Left
        {
            get { return _left; }
        }

        public BoundRelation Right
        {
            get { return _right; }
        }

        public ImmutableArray<ValueSlot> Outputs
        {
            get { return _outputs; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _outputs;
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _outputs;
        }

        public BoundCombinedRelation Update(BoundQueryCombinator combinator, BoundRelation left, BoundRelation right, IEnumerable<ValueSlot> outputs)
        {
            var newOutputs = outputs.ToImmutableArray();

            if (combinator == _combinator && left == _left && right == _right && newOutputs == _outputs)
                return this;

            return new BoundCombinedRelation(combinator, left, right, newOutputs);
        }
    }
}