#nullable enable

namespace NQuery.Refactor.Algebra
{
    // Base of the logical row-expression language: scalar expressions evaluated
    // against a row of value slots. Unlike BoundExpression (the semantic mirror),
    // this language has only slot references as leaves -- never column-by-name --
    // and models subqueries as nodes that hold a LogicalOperator rather than a
    // bound relation, so a logical tree contains no bound islands.
    internal abstract class LogicalExpression
    {
        public abstract LogicalExpressionKind Kind { get; }

        public abstract Type Type { get; }
    }
}
