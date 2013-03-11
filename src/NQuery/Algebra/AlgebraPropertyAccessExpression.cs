using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraPropertyAccessExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _target;
        private readonly PropertySymbol _symbol;

        public AlgebraPropertyAccessExpression(AlgebraExpression target, PropertySymbol symbol)
        {
            _target = target;
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.PropertyAccessExpression; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }

        public AlgebraExpression Target
        {
            get { return _target; }
        }

        public PropertySymbol Symbol
        {
            get { return _symbol; }
        }
    }
}