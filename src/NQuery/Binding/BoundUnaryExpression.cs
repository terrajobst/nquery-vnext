using System;
using System.Linq;

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
                return Result.Selected == null
                           ? TypeFacts.Unknown
                           : Result.Selected.Signature.ReturnType;
            }
        }

        public UnaryOperatorKind OperatorKind { get; }

        public OverloadResolutionResult<UnaryOperatorSignature> Result { get; }

        public BoundExpression Expression { get; }

        public BoundUnaryExpression Update(UnaryOperatorKind operatorKind, OverloadResolutionResult<UnaryOperatorSignature> result, BoundExpression expression)
        {
            if (operatorKind == OperatorKind && result == Result && expression == Expression)
                return this;

            return new BoundUnaryExpression(operatorKind, result, expression);
        }

        public override string ToString()
        {
            var unaryOperatorKind = Result.Candidates.First().Signature.Kind;
            return $"{unaryOperatorKind.ToDisplayName()}({Expression})";
        }
    }
}