#nullable enable

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class LogicalVariableExpression : LogicalExpression
    {
        public LogicalVariableExpression(VariableSymbol symbol)
        {
            Symbol = symbol;
        }

        public override LogicalExpressionKind Kind => LogicalExpressionKind.Variable;

        public override Type Type => Symbol.Type;

        public VariableSymbol Symbol { get; }
    }
}
