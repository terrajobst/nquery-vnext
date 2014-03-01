using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Symbols;

using System.Linq;

namespace NQuery.Plan
{
    internal sealed class PlanBuilder
    {
        public static Iterator Build(BoundRelation relation)
        {
            var builder = new PlanBuilder();
            var result = builder.BuildRelation(relation);
            return result.Iterator;
        }

        private static Func<bool> BuildPredicate(BoundExpression predicate, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            if (predicate == null)
                return () => true;

            return BuildExpression<bool>(predicate, rowBuffer, valueSlotMapping);
        }

        private static Func<object> BuildValue(BoundExpression expression, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            return BuildExpression<object>(expression, rowBuffer, valueSlotMapping);
        }

        private static Func<T> BuildExpression<T>(BoundExpression expression, RowBuffer rowBuffer, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            var rowBufferProvider = new Func<RowBuffer>(() => rowBuffer);
            var valueSlotSettings = new ValueSlotSettings(valueSlotMapping, rowBufferProvider);
            return ExpressionBuilder.BuildExpression<T>(expression, valueSlotSettings);
        }

        private IteratorResult BuildRelation(BoundRelation relation)
        {
            switch (relation.Kind)
            {
                case BoundNodeKind.ConstantRelation:
                    return BuildConstant((BoundConstantRelation)relation);
                case BoundNodeKind.TableRelation:
                    return BuildTable((BoundTableRelation)relation);
                case BoundNodeKind.JoinRelation:
                    return BuildJoin((BoundJoinRelation)relation);
                case BoundNodeKind.FilterRelation:
                    return BuildFilter((BoundFilterRelation)relation);
                case BoundNodeKind.ComputeRelation:
                    return BuildCompute((BoundComputeRelation)relation);
                case BoundNodeKind.TopRelation:
                    return BuildTop((BoundTopRelation)relation);
                case BoundNodeKind.SortRelation:
                    return BuildSort((BoundSortRelation)relation);
                case BoundNodeKind.CombinedRelation:
                    return BuildCombinedQuery((BoundCombinedRelation)relation);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return BuildGroupByAndAggregation((BoundGroupByAndAggregationRelation)relation);
                case BoundNodeKind.ProjectRelation:
                    return BuildProject((BoundProjectRelation)relation);
                default:
                    throw new ArgumentOutOfRangeException("relation", string.Format("Unknown relation kind: {0}.", relation.Kind));
            }
        }

        private static IteratorResult BuildConstant(BoundConstantRelation relation)
        {
            var result = new ConstantIterator();
            var mapping = ImmutableDictionary.Create<ValueSlot, int>();
            return new IteratorResult(result, mapping);
        }

        private static IteratorResult BuildTable(BoundTableRelation relation)
        {
            var schemaTableSymbol = (SchemaTableSymbol) relation.TableInstance.Table;
            var tableDefinition = schemaTableSymbol.Definition;
            // TODO: We proably want to only define the needed columns here.
            var columnInstances = relation.TableInstance.ColumnInstances;
            var definedValues = columnInstances.Select(ci => ci.Column)
                                               .Cast<SchemaColumnSymbol>()
                                               .Select(c => c.Definition)
                                               .ToArray();
            var result = new TableIterator(tableDefinition, definedValues);
            var mapping = ImmutableDictionary.Create(columnInstances.Select((c, i) => new KeyValuePair<ValueSlot, int>(c.ValueSlot, i)));
            return new IteratorResult(result, mapping);
        }

        private IteratorResult BuildJoin(BoundJoinRelation relation)
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

        private IteratorResult BuildFilter(BoundFilterRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var valueSlotMapping = inputResult.ValueSlotMapping;
            var predicate = BuildPredicate(relation.Condition, input.RowBuffer, valueSlotMapping);
            var iterator = new FilterIterator(input, predicate);
            return new IteratorResult(iterator, valueSlotMapping);
        }

        private IteratorResult BuildCompute(BoundComputeRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputRowBuffer = input.RowBuffer;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var inputCount = inputValueSlotMapping.Count;
            var definedValue = relation.DefinedValues
                                       .Select(dv => BuildValue(dv.Expression, inputRowBuffer, inputValueSlotMapping))
                                       .ToArray();
            var additionalValueSlotMappings = relation.DefinedValues.Select((dv, i) => new KeyValuePair<ValueSlot, int>(dv.ValueSlot, i + inputCount));
            var outputValueSlotMapping = inputValueSlotMapping.AddRange(additionalValueSlotMappings);
            var outputItererator = new ComputeScalarIterator(input, definedValue);
            return new IteratorResult(outputItererator, outputValueSlotMapping);
        }

        private IteratorResult BuildTop(BoundTopRelation relation)
        {
            return relation.TieEntries.Any()
                       ? BuildTopWithTies(relation)
                       : BuildTopWithoutTies(relation);
        }

        private IteratorResult BuildTopWithTies(BoundTopRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var tieEntries = relation.TieEntries.Select(t => inputValueSlotMapping[t.ValueSlot]).ToArray();
            var tieComparers = relation.TieEntries.Select(t => t.Comparer).ToArray();
            var outputIterator = new TopWithTiesIterator(input, relation.Limit, tieEntries, tieComparers);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildTopWithoutTies(BoundTopRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var outputIterator = new TopIterator(input, relation.Limit);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildSort(BoundSortRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var input = inputResult.Iterator;
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var sortEntries = relation.SortedValues
                                      .Select(v => inputValueSlotMapping[v.ValueSlot])
                                      .ToArray();
            var comparers = relation.SortedValues.Select(v => v.Comparer).ToArray();
            var outputIterator = new SortIterator(input, sortEntries, comparers);
            return new IteratorResult(outputIterator, inputValueSlotMapping);
        }

        private IteratorResult BuildCombinedQuery(BoundCombinedRelation relation)
        {
            // TODO: We should handle UNION ALL, EXCEPT and INTERSECT differently

            var inputs = new[]
                             {
                                 BuildRelation(relation.Left).Iterator,
                                 BuildRelation(relation.Right).Iterator
                             };

            var outputValueSlotCount = relation.Outputs.Count;
            var outputValueSlotMapping = relation.Outputs
                                                 .Select((v, i) => new KeyValuePair<ValueSlot, int>(v, i))
                                                 .ToImmutableDictionary();
            var outputIterator = new ConcatenationIterator(inputs, outputValueSlotCount);
            return new IteratorResult(outputIterator, outputValueSlotMapping);
        }

        private IteratorResult BuildGroupByAndAggregation(BoundGroupByAndAggregationRelation relation)
        {
            throw new NotImplementedException();
        }

        private IteratorResult BuildProject(BoundProjectRelation relation)
        {
            var inputResult = BuildRelation(relation.Input);
            var inputValueSlotMapping = inputResult.ValueSlotMapping;
            var outputIndices  = relation.Outputs.Select(vs => inputValueSlotMapping[vs]).ToArray();
            var outputIterator = new ProjectionIterator(inputResult.Iterator, outputIndices);
            var outputValueMapping = relation.Outputs
                                             .Select((vs, i) => new KeyValuePair<ValueSlot, int>(vs, i))
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