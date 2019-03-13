#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Iterators;

namespace NQuery
{
    public sealed class CompiledQuery
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
            var columnNamesAndTypes = _query.OutputColumns.Select(c => Tuple.Create(c.Name, c.Type.ToOutputType())).ToImmutableArray();
            var iterator = IteratorBuilder.Build(_query.Relation);
            return new QueryReader(iterator, columnNamesAndTypes, schemaOnly);
        }

        public ExpressionEvaluator CreateExpressionEvaluator()
        {
            var expressionType = _query.OutputColumns.First().ValueSlot.Type;
            var expression = CreateExpression();
            return new ExpressionEvaluator(expressionType, expression);
        }

        private Func<object?> CreateExpression()
        {
            // In the general case evaluating an expression means evaluating the query.
            // That's because an expression might contain sub queries.
            //
            // However, in many cases expressions don't contain sub queries and
            // evaluting a query is considerably more expensive than just evaluating an
            // expression directly.
            //
            // Thus, let's first check whether the query is trivia, i.e. only contains
            // a compute node whose input is a constant relation. That means we can
            // just evaluate the expression being defined.

            var projectRelation = _query.Relation as BoundProjectRelation;
            if (projectRelation != null)
            {
                var computeRelation = projectRelation.Input as BoundComputeRelation;
                if (computeRelation != null && computeRelation.Input is BoundConstantRelation)
                {
                    // This means this is a trivial query.
                    return CreateTrivialExpression(computeRelation);
                }
            }

            // Too bad, we need to evaluate the expression as a regular query.
            return EvaluteQueryAsExpression;
        }

        private static Func<object?> CreateTrivialExpression(BoundComputeRelation computeRelation)
        {
            var computedValue = computeRelation.DefinedValues.First();
            return ExpressionBuilder.BuildFunction(computedValue.Expression);
        }

        private object? EvaluteQueryAsExpression()
        {
            using (var reader = CreateReader())
            {
                return !reader.Read() || reader.ColumnCount == 0
                    ? null
                    : reader[0];
            }
        }
    }
}