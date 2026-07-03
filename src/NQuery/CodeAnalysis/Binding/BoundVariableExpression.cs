using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variableSymbol)
    {
        ThrowIfNull(variableSymbol);

        Symbol = variableSymbol;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.VariableExpression; }
    }

    public override Type Type
    {
        get { return Symbol.Type; }
    }

    public VariableSymbol Symbol { get; }

    public override string ToString()
    {
        return $"@{Symbol.Name}";
    }
}
