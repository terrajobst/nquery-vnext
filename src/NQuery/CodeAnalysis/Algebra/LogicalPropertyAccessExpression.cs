using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalPropertyAccessExpression : LogicalExpression
{
    public LogicalPropertyAccessExpression(LogicalExpression target, PropertySymbol symbol)
    {
        ThrowIfNull(target);
        ThrowIfNull(symbol);

        Target = target;
        Symbol = symbol;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.PropertyAccess;

    public override Type Type => Symbol.Type;

    public LogicalExpression Target { get; }

    public PropertySymbol Symbol { get; }
}
