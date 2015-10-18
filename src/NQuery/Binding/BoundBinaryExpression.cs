using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly BoundExpression _left;
        private readonly BinaryOperatorKind _operatorKind;
        private readonly OverloadResolutionResult<BinaryOperatorSignature> _result;
        private readonly BoundExpression _right;

        public BoundBinaryExpression(BoundExpression left, BinaryOperatorKind operatorKind, OverloadResolutionResult<BinaryOperatorSignature> result, BoundExpression right)
        {
            _left = left;
            _operatorKind = operatorKind;
            _result = result;
            _right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.BinaryExpression; }
        }

        public override Type Type
        {
            get
            {
                return _result.Selected == null
                           ? TypeFacts.Unknown
                           : _result.Selected.Signature.ReturnType;
            }
        }

        public BoundExpression Left
        {
            get { return _left; }
        }

        public OverloadResolutionResult<BinaryOperatorSignature> Result
        {
            get { return _result; }
        }


        public BinaryOperatorKind OperatorKind
        {
            get { return _operatorKind; }
        }

        public BoundExpression Right
        {
            get { return _right; }
        }

        public BoundBinaryExpression Update(BoundExpression left, BinaryOperatorKind operatorKind, OverloadResolutionResult<BinaryOperatorSignature> result, BoundExpression right)
        {
            if (left == _left && operatorKind == _operatorKind && result == _result && right == _right)
                return this;

            return new BoundBinaryExpression(left, operatorKind, result, right);
        }

        public override string ToString()
        {
            var kind = _result.Candidates.First().Signature.Kind;
            return $"({_left} {kind.ToDisplayName()} {_right})";
        }
    }
}