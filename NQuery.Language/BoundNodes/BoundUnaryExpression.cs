using System;
using NQuery.Language.Binding;

namespace NQuery.Language.BoundNodes
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
                           ? WellKnownTypes.Unknown
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
    }
}