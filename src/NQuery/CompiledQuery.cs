using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Iterators;

namespace NQuery
{
    // Engine-agnostic surface. How the reader's iterator is produced (BuildIterator) and
    // where the output columns come from (OutputColumns) differ per engine and live in
    // CompiledQuery.Baseline.cs / CompiledQuery.NewEngine.cs.
    //
    // The legacy BoundQuery artifact is kept here because bare-expression compilation
    // (Expression<T>, e.g. aggregate type resolution) always runs on the legacy engine in
    // both builds -- the new pipeline only handles top-level queries. CreateExpressionEvaluator
    // is therefore only ever reached for an expression-backed instance (where _query is set).
    public sealed partial class CompiledQuery
    {
        private readonly BoundQuery _query;

        internal CompiledQuery(BoundQuery query)
        {
            _query = query;
        }

        public QueryReader CreateReader()
        {
            return CreateReader(false);
        }

        public QueryReader CreateSchemaReader()
        {
            return CreateReader(true);
        }

        private QueryReader CreateReader(bool schemaOnly)
        {
            var columnNamesAndTypes = OutputColumns.Select(c => (c.Name, c.Type.ToOutputType())).ToImmutableArray();
            return new QueryReader(BuildIterator(), columnNamesAndTypes, schemaOnly);
        }

        public ExpressionEvaluator CreateExpressionEvaluator()
        {
            // If the query is empty, just return null
            if (_query.OutputColumns.Length == 0)
                return new ExpressionEvaluator(typeof(object), () => null);

            var expressionType = _query.OutputColumns.First().ValueSlot.Type;
            var expression = CreateExpression();
            return new ExpressionEvaluator(expressionType, expression);
        }

        private Func<object> CreateExpression()
        {
            // In the general case evaluating an expression means evaluating the query.
            // That's because an expression might contain sub queries.
            //
            // However, in many cases expressions don't contain sub queries and
            // evaluating a query is considerably more expensive than just evaluating an
            // expression directly.
            //
            // Thus, let's first check whether the query is trivial, i.e. only contains
            // a compute node whose input is a constant relation. That means we can
            // just evaluate the expression being defined.

            if (_query.Relation is BoundProjectRelation projectRelation)
            {
                var computeRelation = projectRelation.Input as BoundComputeRelation;
                if (computeRelation?.Input is BoundConstantRelation)
                {
                    // This means this is a trivial query.
                    return CreateTrivialExpression(computeRelation);
                }
            }

            // Too bad, we need to evaluate the expression as a regular query.
            return EvaluateQueryAsExpression;
        }

        private static Func<object> CreateTrivialExpression(BoundComputeRelation computeRelation)
        {
            var computedValue = computeRelation.DefinedValues.First();
            return ExpressionBuilder.BuildFunction(computedValue.Expression);
        }

        private object EvaluateQueryAsExpression()
        {
            using var reader = CreateReader();
            return !reader.Read() || reader.ColumnCount == 0
                ? null
                : reader[0];
        }
    }
}
