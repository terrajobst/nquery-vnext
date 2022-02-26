using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundPropertyAccessExpression : BoundExpression
    {
        public BoundPropertyAccessExpression(BoundExpression target, PropertySymbol propertySymbol)
        {
            Target = target;
            Symbol = propertySymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.PropertyAccessExpression; }
        }

        public override Type Type
        {
            get { return Symbol.Type; }
        }

        public PropertySymbol Symbol { get; }

        public BoundExpression Target { get; }

        public PropertySymbol PropertySymbol
        {
            get { return Symbol; }
        }

        public BoundPropertyAccessExpression Update(BoundExpression target, PropertySymbol propertySymbol)
        {
            if (target == Target && propertySymbol == Symbol)
                return this;

            return new BoundPropertyAccessExpression(target, propertySymbol);
        }

        public override string ToString()
        {
            return $"{Target}.{Symbol.Name}";
        }
    }
}