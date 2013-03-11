using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraVariableExpression : AlgebraExpression
    {
        private readonly VariableSymbol _symbol;

        public AlgebraVariableExpression(VariableSymbol symbol)
        {
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.VariableExpression; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }

        public VariableSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}