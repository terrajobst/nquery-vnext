using System.Linq.Expressions;

namespace NQuery.Metadata;

internal sealed class ExpressionMethodDefinition : MethodDefinition
{
    private readonly LambdaExpression _expression;

    public ExpressionMethodDefinition(string name, LambdaExpression expression)
        : base(name, expression.ReturnType, GetParameters(expression))
    {
        _expression = expression;
    }

    private static IEnumerable<ParameterDefinition> GetParameters(LambdaExpression expression)
    {
        // The first lambda parameter is the instance; the rest are the method's parameters.
        return expression.Parameters.Skip(1).Select(p => ParameterDefinition.Create(p.Name!, p.Type));
    }

    internal override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
    {
        return ExpressionInliner.Inline(_expression, new[] { instance }.Concat(arguments));
    }
}
