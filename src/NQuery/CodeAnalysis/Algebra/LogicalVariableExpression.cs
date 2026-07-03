using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalVariableExpression : LogicalExpression
{
    public LogicalVariableExpression(VariableSymbol symbol)
    {
        ThrowIfNull(symbol);

        Symbol = symbol;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.Variable;

    public override Type Type => Symbol.Type;

    public VariableSymbol Symbol { get; }
}
