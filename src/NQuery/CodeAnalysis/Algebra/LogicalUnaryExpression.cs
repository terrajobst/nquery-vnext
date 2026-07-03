using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalUnaryExpression : LogicalExpression
{
    public LogicalUnaryExpression(UnaryOperatorKind operatorKind, OverloadResolutionResult<UnaryOperatorSignature> result, LogicalExpression expression)
    {
        ThrowIfNull(result);
        ThrowIfNull(expression);

        OperatorKind = operatorKind;
        Result = result;
        Expression = expression;
    }

    public override LogicalExpressionKind Kind => LogicalExpressionKind.Unary;

    public override Type Type => Result.Selected is null ? TypeFacts.Unknown : Result.Selected.Signature.ReturnType;

    public UnaryOperatorKind OperatorKind { get; }

    public OverloadResolutionResult<UnaryOperatorSignature> Result { get; }

    public LogicalExpression Expression { get; }
}
