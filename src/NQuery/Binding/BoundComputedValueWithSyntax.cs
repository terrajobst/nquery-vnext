using System;

using NQuery.Syntax;

namespace NQuery.Binding
{
    internal struct BoundComputedValueWithSyntax
    {
        public BoundComputedValueWithSyntax(ExpressionSyntax syntax, BoundExpression expression, ValueSlot result)
        {
            Syntax = syntax;
            Expression = expression;
            Result = result;
        }

        public ExpressionSyntax Syntax { get; }

        public BoundExpression Expression { get; }

        public ValueSlot Result { get; }
    }
}