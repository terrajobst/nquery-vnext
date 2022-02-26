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
                return Value == null
                           ? TypeFacts.Null
                           : Value.GetType();
            }
        }

        public object Value { get; }

        public override string ToString()
        {
            if (Value == null)
                return @"NULL";

            if (Value is string)
                return $"'{Value}'"; // TODO: We should escape this

            if (Value is DateTime)
                return $"#{Value}#";

            return Value.ToString();
        }
    }
}