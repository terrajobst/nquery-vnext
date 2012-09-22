using System;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundPropertyAccessExpression : BoundExpression
    {
        private readonly BoundExpression _target;
        private readonly PropertySymbol _propertySymbol;

        public BoundPropertyAccessExpression(BoundExpression target, PropertySymbol propertySymbol)
        {
            _target = target;
            _propertySymbol = propertySymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.PropertyAccessExpression; }
        }

        public override Type Type
        {
            get { return _propertySymbol.Type; }
        }

        public PropertySymbol Symbol
        {
            get { return _propertySymbol; }
        }

        public BoundExpression Target
        {
            get { return _target; }
        }

        public PropertySymbol PropertySymbol
        {
            get { return _propertySymbol; }
        }

        public BoundExpression Update(BoundExpression target)
        {
            return _target == target
                       ? this
                       : new BoundPropertyAccessExpression(target, _propertySymbol);
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", _target, Symbol.Name);
        }
    }
}