namespace NQuery.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundExpression left, BinaryOperatorKind operatorKind, OverloadResolutionResult<BinaryOperatorSignature> result, BoundExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Result = result;
            Right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.BinaryExpression; }
        }

        public override Type Type
        {
            get
            {
                return Result.Selected == null
                           ? TypeFacts.Unknown
                           : Result.Selected.Signature.ReturnType;
            }
        }

        public BoundExpression Left { get; }

        public BinaryOperatorKind OperatorKind { get; }

        public OverloadResolutionResult<BinaryOperatorSignature> Result { get; }

        public BoundExpression Right { get; }

        public BoundBinaryExpression Update(BoundExpression left, BinaryOperatorKind operatorKind, OverloadResolutionResult<BinaryOperatorSignature> result, BoundExpression right)
        {
            if (left == Left && operatorKind == OperatorKind && result == Result && right == Right)
                return this;

            return new BoundBinaryExpression(left, operatorKind, result, right);
        }

        public override string ToString()
        {
            var kind = Result.Candidates.First().Signature.Kind;
            return $"({Left} {kind.ToDisplayName()} {Right})";
        }
    }
}