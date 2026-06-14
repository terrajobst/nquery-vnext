namespace NQuery.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(UnaryOperatorKind operatorKind, OverloadResolutionResult<UnaryOperatorSignature> result, BoundExpression expression)
        {
            OperatorKind = operatorKind;
            Expression = expression;
            Result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnaryExpression; }
        }

        public override Type Type
        {
            get
            {
                return Result.Selected is null
                           ? TypeFacts.Unknown
                           : Result.Selected.Signature.ReturnType;
            }
        }

        public UnaryOperatorKind OperatorKind { get; }

        public OverloadResolutionResult<UnaryOperatorSignature> Result { get; }

        public BoundExpression Expression { get; }

        public override string ToString()
        {
            var unaryOperatorKind = Result.Candidates.First().Signature.Kind;
            return $"{unaryOperatorKind.ToDisplayName()}({Expression})";
        }
    }
}