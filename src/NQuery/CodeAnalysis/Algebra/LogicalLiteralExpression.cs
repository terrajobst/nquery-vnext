namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalLiteralExpression : LogicalExpression
{
    public LogicalLiteralExpression(object? value)
    {
        Value = value;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.Literal;

    public override Type Type => Value is null ? TypeFacts.Null : Value.GetType();

    public object? Value { get; }
}
