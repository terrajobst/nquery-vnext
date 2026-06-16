namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalIsNullExpression : LogicalExpression
{
    public LogicalIsNullExpression(LogicalExpression expression)
    {
        Expression = expression;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.IsNull;

    public override Type Type => typeof(bool);

    public LogicalExpression Expression { get; }
}
