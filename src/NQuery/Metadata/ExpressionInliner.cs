using System.Collections.Immutable;
using System.Linq.Expressions;

namespace NQuery.Metadata;

// Inlines a user-supplied lambda into the query's expression tree by replacing the lambda's
// parameters with the engine-supplied argument expressions. This is what lets functions,
// methods, and properties be defined via LINQ expressions: instead of emitting a call to a
// delegate, the lambda body is spliced directly into the row evaluator.
internal static class ExpressionInliner
{
    public static Expression Inline(LambdaExpression lambda, IEnumerable<Expression> arguments)
    {
        var args = arguments.ToImmutableArray();

        var map = new Dictionary<ParameterExpression, Expression>();
        for (var i = 0; i < lambda.Parameters.Count; i++)
        {
            var parameter = lambda.Parameters[i];
            var argument = args[i];
            map[parameter] = argument.Type == parameter.Type
                ? argument
                : Expression.Convert(argument, parameter.Type);
        }

        return new ParameterReplacer(map).Visit(lambda.Body);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly IReadOnlyDictionary<ParameterExpression, Expression> _map;

        public ParameterReplacer(IReadOnlyDictionary<ParameterExpression, Expression> map)
        {
            _map = map;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _map.TryGetValue(node, out var replacement) ? replacement : base.VisitParameter(node);
        }
    }
}
