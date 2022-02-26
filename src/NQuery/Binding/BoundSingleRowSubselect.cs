namespace NQuery.Binding
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        public BoundSingleRowSubselect(ValueSlot value, BoundRelation relation)
        {
            Value = value;
            Relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SingleRowSubselect; }
        }

        public override Type Type
        {
            get { return Value.Type; }
        }

        public ValueSlot Value { get; }

        public BoundRelation Relation { get; }

        public BoundSingleRowSubselect Update(ValueSlot value, BoundRelation relation)
        {
            if (value == Value && relation == Relation)
                return this;

            return new BoundSingleRowSubselect(value, relation);
        }

        public override string ToString()
        {
            return $"({Relation})";
        }
    }
}