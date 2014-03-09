using System;

using NQuery.Syntax;

namespace NQuery.Binding
{
    internal struct BoundComputedValueWithSyntax
    {
        private readonly ExpressionSyntax _syntax;
        private readonly BoundExpression _expression;
        private readonly ValueSlot _result;

        public BoundComputedValueWithSyntax(ExpressionSyntax syntax, BoundExpression expression, ValueSlot result)
        {
            _syntax = syntax;
            _expression = expression;
            _result = result;
        }

        public ExpressionSyntax Syntax
        {
            get { return _syntax; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public ValueSlot Result
        {
            get { return _result; }
        }
    }
}