using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        private readonly BoundExpression _left;
        private readonly OverloadResolutionResult<BinaryOperatorSignature> _result;
        private readonly BoundExpression _right;

        public BoundBinaryExpression(BoundExpression left, OverloadResolutionResult<BinaryOperatorSignature> result, BoundExpression right)
        {
            _left = left;
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

        public BoundExpression Right
        {
            get { return _right; }
        }

        public BoundExpression Update(BoundExpression left, BoundExpression right)
        {
            return (left == _left && right == _right)
                       ? this
                       : new BoundBinaryExpression(left, _result, right);
        }

        public override string ToString()
        {
            var kind = _result.Candidates.First().Signature.Kind;
            return string.Format("({0} {1} {2})", _left, kind.ToDisplayName(), _right);
        }
    }
}