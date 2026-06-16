using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

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

    public override string ToString()
    {
        return $"{Target}.{Symbol.Name}";
    }
}
