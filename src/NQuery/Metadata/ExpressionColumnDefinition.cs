using System.Linq.Expressions;

namespace NQuery.Metadata;

internal sealed class ExpressionColumnDefinition : ColumnDefinition
{
    private readonly LambdaExpression _expression;

    public ExpressionColumnDefinition(string name, Type dataType, LambdaExpression expression)
        : base(name, dataType)
    {
        _expression = expression;
    }

    internal override Expression CreateInvocation(Expression instance)
    {
        var value = ExpressionInliner.Inline(_expression, new[] { instance });
        return value.Type == typeof(object) ? value : Expression.Convert(value, typeof(object));
    }
}
