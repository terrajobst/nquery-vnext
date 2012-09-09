using System;
using System.Reflection;
using NQuery.Language.Binding;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly MethodInfo _methodInfo;

        public BoundUnaryExpression(BoundExpression expression, MethodInfo methodInfo)
        {
            _expression = expression;
            _methodInfo = methodInfo;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.UnaryExpression; }
        }

        public override Type Type
        {
            get
            {
                return _methodInfo == null
                           ? WellKnownTypes.Unknown
                           : _methodInfo.ReturnType;
            }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }
    }
}