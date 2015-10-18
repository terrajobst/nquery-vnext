using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly OverloadResolutionResult<UnaryOperatorSignature> _result;
        private readonly BoundExpression _expression;

        public BoundUnaryExpression(OverloadResolutionResult<UnaryOperatorSignature> result, BoundExpression expression)
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

        public OverloadResolutionResult<UnaryOperatorSignature> Result
        {
            get { return _result; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public BoundUnaryExpression Update(OverloadResolutionResult<UnaryOperatorSignature> result, BoundExpression expression)
        {
            if (result == _result && expression == _expression)
                return this;
            
            return new BoundUnaryExpression(result, expression);
        }

        public override string ToString()
        {
            var unaryOperatorKind = _result.Candidates.First().Signature.Kind;
            return $"{unaryOperatorKind.ToDisplayName()}({_expression})";
        }
    }
}