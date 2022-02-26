namespace NQuery.Binding
{
    internal sealed class BoundFilterRelation : BoundRelation
    {
        public BoundFilterRelation(BoundRelation input, BoundExpression condition)
        {
            Input = input;
            Condition = condition;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.FilterRelation; }
        }

        public BoundRelation Input { get; }

        public BoundExpression Condition { get; }

        public BoundFilterRelation Update(BoundRelation input, BoundExpression condition)
        {
            if (input == Input && condition == Condition)
                return this;

            return new BoundFilterRelation(input, condition);
        }

        public BoundFilterRelation WithInput(BoundRelation input)
        {
            return Update(input, Condition);
        }

        public BoundFilterRelation WithCondition(BoundExpression condition)
        {
            return Update(Input, condition);
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