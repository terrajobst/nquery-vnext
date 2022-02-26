namespace NQuery.Binding
{
    internal sealed class BoundConstantRelation : BoundRelation
    {
        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConstantRelation; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }
    }
}