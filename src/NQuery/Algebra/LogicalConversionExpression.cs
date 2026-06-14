#nullable enable

namespace NQuery.Algebra;

internal sealed class LogicalConversionExpression : LogicalExpression
{
    private readonly Type _type;

    public LogicalConversionExpression(LogicalExpression expression, Type type, Conversion conversion)
    {
        Expression = expression;
        _type = type;
        Conversion = conversion;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.Conversion;

    public override Type Type => _type;

    public LogicalExpression Expression { get; }

    public Conversion Conversion { get; }
}
