namespace NQuery.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.LiteralExpression; }
        }

        public override Type Type
        {
            get
            {
                return Value is null
                           ? TypeFacts.Null
                           : Value.GetType();
            }
        }

        public object Value { get; }

        public override string ToString()
        {
            return Value switch
            {
                null => @"NULL",
                string => $"'{Value}'",
                DateTime => $"#{Value}#",
                _ => Value.ToString()
            };
        }
    }
}