using NQuery.CodeAnalysis.Syntax;

namespace NQuery.CodeAnalysis.Binding;

internal struct BoundComputedValueWithSyntax
{
    public BoundComputedValueWithSyntax(ExpressionSyntax syntax, BoundExpression expression, IBoundValue result)
    {
        Syntax = syntax;
        Expression = expression;
        Result = result;
    }

    public ExpressionSyntax Syntax { get; }

    public BoundExpression Expression { get; }

    public IBoundValue Result { get; }
}
