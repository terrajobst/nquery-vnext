using NQuery.Syntax;

using NQuery.Binding;

namespace NQuery.AlgebraBinding
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