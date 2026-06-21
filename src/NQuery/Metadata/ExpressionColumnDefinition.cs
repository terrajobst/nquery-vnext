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

    // Returns the accessor's value in its own CLR type. A typed accessor
    // (Func<TRow, TValue>) therefore avoids boxing; the object-typed overload
    // (Func<TRow, object>, e.g. a DataRow indexer) naturally stays object and the row
    // writer unboxes it into the typed slot.
    internal override Expression CreateInvocation(Expression instance)
    {
        return ExpressionInliner.Inline(_expression, new[] { instance });
    }
}
