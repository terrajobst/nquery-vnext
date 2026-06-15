using System.Linq.Expressions;

namespace NQuery.Metadata;

internal sealed class ExpressionPropertyDefinition : PropertyDefinition
{
    private readonly LambdaExpression _expression;

    public ExpressionPropertyDefinition(string name, Type type, LambdaExpression expression)
        : base(name, type)
    {
        _expression = expression;
    }

    internal override Expression CreateInvocation(Expression instance)
    {
        var value = ExpressionInliner.Inline(_expression, new[] { instance });
        return value.Type == Type ? value : Expression.Convert(value, Type);
    }
}
