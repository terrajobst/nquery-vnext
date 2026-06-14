using System.Collections.Immutable;

using NQuery.Refactor.Emit;
using NQuery.Refactor.Iterators;
using NQuery.Symbols;

namespace NQuery
{
    // Every compilation -- top-level query and bare expression alike -- is an emitted
    // ExecutablePlan. (A bare expression is wrapped in a one-row projection by the algebrizer,
    // so it plans and emits like any other query.)
    public sealed class CompiledQuery
    {
        private readonly ExecutablePlan _plan;

        internal CompiledQuery(ExecutablePlan plan)
        {
            _plan = plan;
        }

        private ImmutableArray<QueryColumnInstanceSymbol> OutputColumns => _plan.OutputColumns;

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

        private Iterator BuildIterator()
        {
            return _plan.CreateIterator();
        }

        public ExpressionEvaluator CreateExpressionEvaluator()
        {
            // If the query is empty, just return null
            if (OutputColumns.Length == 0)
                return new ExpressionEvaluator(typeof(object), () => null);

            var expressionType = OutputColumns[0].Type;
            return new ExpressionEvaluator(expressionType, EvaluateQueryAsExpression);
        }

        private object EvaluateQueryAsExpression()
        {
            // Evaluating an expression means evaluating its query: a bare expression is wrapped
            // into a one-row projection by the algebrizer and emitted like any other query, so we
            // just run that plan and read the single output value.
            using var reader = CreateReader();
            return !reader.Read() || reader.ColumnCount == 0
                ? null
                : reader[0];
        }
    }
}
