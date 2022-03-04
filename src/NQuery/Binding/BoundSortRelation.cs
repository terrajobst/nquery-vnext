using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundSortRelation : BoundRelation
    {
        public BoundSortRelation(bool isDistinct, BoundRelation input, IEnumerable<BoundComparedValue> sortedValues)
        {
            Input = input;
            SortedValues = sortedValues.ToImmutableArray();
            IsDistinct = isDistinct;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SortRelation; }
        }

        public BoundRelation Input { get; }

        public ImmutableArray<BoundComparedValue> SortedValues { get; }

        public bool IsDistinct { get; }

        public BoundSortRelation Update(bool isDistinct, BoundRelation input, IEnumerable<BoundComparedValue> sortedValues)
        {
            var newSortedValues = sortedValues.ToImmutableArray();

            if (isDistinct == IsDistinct && input == Input && newSortedValues == SortedValues)
                return this;

            return new BoundSortRelation(isDistinct, input, newSortedValues);
        }

        public BoundSortRelation WithInput(BoundRelation input)
        {
            return Update(IsDistinct, input, SortedValues);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Input.GetOutputValues();
        }
    }
}