using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.Symbols;

using System.Linq;

namespace NQuery.Plan
{
    internal sealed class PlanBuilder
    {
        public static Iterator Build(AlgebraRelation relation)
        {
            var builder = new PlanBuilder();
            var result = builder.BuildRelation(relation);
            return result.Iterator;
        }

        private static Func<bool> BuildPredicate(AlgebraExpression predicate, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            if (predicate == null)
                return () => true;

            return BuildExpression<bool>(predicate, rowBuffer, valueSlotMapping);
        }

        private static Func<object> BuildValue(AlgebraExpression expression, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            return BuildExpression<object>(expression, rowBuffer, valueSlotMapping);
        }

        private static Func<T> BuildExpression<T>(AlgebraExpression expression, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            var rowBufferProvider = new Func<RowBuffer>(() => rowBuffer);
            var valueSlotSettings = new ValueSlotSettings(valueSlotMapping, rowBufferProvider);
            return ExpressionBuilder.BuildExpression<T>(expression, valueSlotSettings);
        }

        private IteratorResult BuildRelation(AlgebraRelation relation)
        {
            switch (relation.Kind)
            {
                case AlgebraKind.Constant:
                    return BuildConstant((AlgebraConstantNode)relation);
                case AlgebraKind.Table:
                    return BuildTable((AlgebraTableNode)relation);
                case AlgebraKind.Join:
                    return BuildJoin((AlgebraJoinNode)relation);
                case AlgebraKind.Filter:
                    return BuildFilter((AlgebraFilterNode)relation);
                case AlgebraKind.Compute:
                    return BuildCompute((AlgebraComputeNode)relation);
                case AlgebraKind.Top:
                    return BuildTop((AlgebraTopNode)relation);
                case AlgebraKind.Sort:
                    return BuildSort((AlgebraSortNode)relation);
                case AlgebraKind.BinaryQuery:
                    return BuildCombinedQuery((AlgebraCombinedQuery)relation);
                case AlgebraKind.GroupByAndAggregation:
                    return BuildGroupByAndAggregation((AlgebraGroupByAndAggregation)relation);
                case AlgebraKind.Project:
                    return BuildProject((AlgebraProjectNode)relation);
                default:
                    throw new ArgumentOutOfRangeException("relation", string.Format("Unknown relation kind: {0}.", relation.Kind));
            }
        }

        private static IteratorResult BuildConstant(AlgebraConstantNode relation)
        {
            var result = new ConstantIterator();
            var mapping = ImmutableDictionary.Create<ValueSlot, int>();
            return new IteratorResult(result, mapping);
        }

        private static IteratorResult BuildTable(AlgebraTableNode relation)
        {
            var schemaTableSymbol = (SchemaTableSymbol) relation.Symbol.Table;
            var tableDefinition = schemaTableSymbol.Definition;
            // TODO: We proably want to only define the needed columns here.
            var columnInstances = relation.Symbol.ColumnInstances;
            var definedValues = columnInstances.Select(ci => ci.Column)
                                               .Cast<SchemaColumnSymbol>()
                                               .Select(c => c.Definition)
                                               .ToArray();
            var result = new TableIterator(tableDefinition, definedValues);
            var mapping = ImmutableDictionary.Create(columnInstances.Select((c, i) => new KeyValuePair<ValueSlot, int>(c.ValueSlot, i)));
            return new IteratorResult(result, mapping);
        }

        private IteratorResult BuildJoin(AlgebraJoinNode relation)
        {
            var leftResult = BuildRelation(relation.Left);
            var left = leftResult.Iterator;
            var rightResult = BuildRelation(relation.Right);
            var right = rightResult.Iterator;
            var leftCount = leftResult.ValueSlotMapping.Count;
            var rigthtMappingsWithOffset = rightResult.ValueSlotMapping.Select(kv => new KeyValuePair<ValueSlot, int>(kv.Key, kv.Value + leftCount));
            var outputValueSlotMapping = leftResult.ValueSlotMapping.AddRange(rigthtMappingsWithOffset);
            var outputRowBuffer = new CombinedRowBuffer(left.RowBuffer, right.RowBuffer);
            var predicate = BuildPredicate(relation.Condition, outputRowBuffer, outputValueSlotMapping);
            var outputIterator = new InnerNestedLoopsIterator(left, right, predicate, outputRowBuffer);
            return new IteratorResult(outputIterator, outputValueSlotMapping);
        }

        private IteratorResult BuildFilter(AlgebraFilterNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var valueSlotMapping = inputResult.ValueSlotMapping;
            var predicate = BuildPredicate(relation.Condition, input.RowBuffer, valueSlotMapping);
            var iterator = new FilterIterator(input, predicate);
            return new IteratorResult(iterator, valueSlotMapping);
        }

        private IteratorResult BuildCompute(AlgebraComputeNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputRowBuffer = input.RowBuffer;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var inputCount = inputValueSlotMapping.Count;
            var definedValue = relation.DefinedValues
                                       .Select(dv => BuildValue(dv.Expression, inputRowBuffer, inputValueSlotMapping))
                                       .ToArray();
            var additionalValueSlotMappings = relation.DefinedValues.Select((dv, i) => new KeyValuePair<ValueSlot, int>(dv.Value, i + inputCount));
            var outputValueSlotMapping = inputValueSlotMapping.AddRange(additionalValueSlotMappings);
            var outputItererator = new ComputeScalarIterator(input, definedValue);
            return new IteratorResult(outputItererator, outputValueSlotMapping);
        }

        private IteratorResult BuildTop(AlgebraTopNode relation)
        {
            return relation.WithTies
                       ? BuildTopWithTies(relation)
                       : BuildTopWithoutTies(relation);
        }

        private IteratorResult BuildTopWithTies(AlgebraTopNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var tieEntries = relation.TieEntries.Select(v => inputValueSlotMapping[v]).ToArray();
            var tieComparers = relation.TieComparers;
            var outputIterator = new TopWithTiesIterator(input, relation.Limit, tieEntries, tieComparers);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildTopWithoutTies(AlgebraTopNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var outputIterator = new TopIterator(input, relation.Limit);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildSort(AlgebraSortNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var sortEntries = relation.ValueSlots
                                      .Select(v => inputValueSlotMapping[v])
                                      .ToArray();
            var comparers = relation.Comparers;
            var outputIterator = new SortIterator(input, sortEntries, comparers);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildCombinedQuery(AlgebraCombinedQuery relation)
        {
            throw new NotImplementedException();
        }

        private IteratorResult BuildGroupByAndAggregation(AlgebraGroupByAndAggregation relation)
        {
            throw new NotImplementedException();
        }

        private IteratorResult BuildProject(AlgebraProjectNode relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var outputIndices  = relation.Output.Select(vs => inputValueSlotMapping[vs]).ToArray();
            var outputIterator = new ProjectionIterator(inputResult.Iterator, outputIndices);
            var outputValueMapping = relation.Output
                                             .Select(vs => new KeyValuePair<ValueSlot, int>(vs, inputValueSlotMapping[vs]))
                                             .ToImmutableDictionary();

            return new IteratorResult(outputIterator, outputValueMapping);
        }

        private struct IteratorResult
        {
            private readonly Iterator _iterator;
            private readonly ImmutableDictionary<ValueSlot, int> _valueSlotMapping;

            public IteratorResult(Iterator iterator, ImmutableDictionary<ValueSlot, int> valueSlotMapping)
            {
                _iterator = iterator;
                _valueSlotMapping = valueSlotMapping;
            }

            public Iterator Iterator
            {
                get { return _iterator; }
            }

            public ImmutableDictionary<ValueSlot, int> ValueSlotMapping
            {
                get { return _valueSlotMapping; }
            }
        }
    }
}