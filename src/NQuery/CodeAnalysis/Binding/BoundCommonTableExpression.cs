using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundCommonTableExpression : BoundNode
{
    public BoundCommonTableExpression(CommonTableExpressionSymbol tableSymbol)
    {
        ThrowIfNull(tableSymbol);

        TableSymbol = tableSymbol;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.CommonTableExpression; }
    }

    public CommonTableExpressionSymbol TableSymbol { get; }
}
