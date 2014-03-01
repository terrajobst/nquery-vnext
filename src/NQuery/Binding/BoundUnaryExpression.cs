using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly OverloadResolutionResult<UnaryOperatorSignature> _result;

        public BoundUnaryExpression(BoundExpression expression, OverloadResolutionResult<UnaryOperatorSignature> result)
        {
            _expression = expression;
            _result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnaryExpression; }
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

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public OverloadResolutionResult<UnaryOperatorSignature> Result
        {
            get { return _result; }
        }

        public BoundUnaryExpression Update(BoundExpression expression, OverloadResolutionResult<UnaryOperatorSignature> result)
        {
            if (expression == _expression && result == _result)
                return this;
            
            return new BoundUnaryExpression(expression, result);
        }

        public override string ToString()
        {
            var unaryOperatorKind = _result.Candidates.First().Signature.Kind;
            return string.Format("{0}({1})", unaryOperatorKind.ToDisplayName(), _expression);
        }
    }
}