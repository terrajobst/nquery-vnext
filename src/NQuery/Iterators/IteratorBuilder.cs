using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Iterators
{
    internal sealed class IteratorBuilder
    {
        public static Iterator Build(BoundRelation relation)
        {
            var builder = new IteratorBuilder();
            return builder.BuildRelation(relation);
        }

        private static IteratorFunction BuildFunction(BoundExpression expression, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            return ExpressionBuilder.BuildIteratorFunction(expression, valueSlotMapping);
        }

        private static IteratorPredicate BuildPredicate(BoundExpression predicate, IReadOnlyDictionary<ValueSlot, int> valueSlotMapping)
        {
            return ExpressionBuilder.BuildIteratorPredicate(predicate, valueSlotMapping);
        }

        private static IReadOnlyDictionary<ValueSlot, int> BuildValueSlotMapping(BoundRelation relation)
        {
            var outputValues = relation.GetOutputValues().ToImmutableArray();
            var dictionary = new Dictionary<ValueSlot, int>(outputValues.Length);
            for (var i = 0; i < outputValues.Length; i++)
            {
                var outputValue = outputValues[i];
                if (!dictionary.ContainsKey(outputValue))
                    dictionary[outputValue] = i;
            }

            return dictionary;
        }

        private Iterator BuildRelation(BoundRelation relation)
        {
            switch (relation.Kind)
            {
                case BoundNodeKind.ConstantRelation:
                    return BuildConstant();
                case BoundNodeKind.TableRelation:
                    return BuildTable((BoundTableRelation)relation);
                case BoundNodeKind.JoinRelation:
                    return BuildJoin((BoundJoinRelation)relation);
                case BoundNodeKind.HashMatchRelation:
                    return BuildHashMatch((BoundHashMatchRelation)relation);
                case BoundNodeKind.FilterRelation:
                    return BuildFilter((BoundFilterRelation)relation);
                case BoundNodeKind.ComputeRelation:
                    return BuildCompute((BoundComputeRelation)relation);
                case BoundNodeKind.TopRelation:
                    return BuildTop((BoundTopRelation)relation);
                case BoundNodeKind.SortRelation:
                    return BuildSort((BoundSortRelation)relation);
                case BoundNodeKind.ConcatenationRelation:
                    return BuildConcatenationRelation((BoundConcatenationRelation)relation);
                case BoundNodeKind.StreamAggregatesRelation:
                    return BuildStreamAggregatesRelation((BoundStreamAggregatesRelation)relation);
                case BoundNodeKind.ProjectRelation:
                    return BuildProject((BoundProjectRelation)relation);
                default:
                    throw new ArgumentOutOfRangeException(nameof(relation), $"Unknown relation kind: {relation.Kind}.");
            }
        }

        private static Iterator BuildConstant()
        {
            return new ConstantIterator();
        }

        private static Iterator BuildTable(BoundTableRelation relation)
        {
            var schemaTableSymbol = (SchemaTableSymbol) relation.TableInstance.Table;
            var tableDefinition = schemaTableSymbol.Definition;
            // TODO: We proably want to only define the needed columns here.
            var columnInstances = relation.TableInstance.ColumnInstances;
            var definedValues = columnInstances.Select(ci => ci.Column)
                                               .Cast<SchemaColumnSymbol>()
                                               .Select(c => c.Definition)
                                               .ToImmutableArray();
            return new TableIterator(tableDefinition, definedValues);
        }

        private Iterator BuildJoin(BoundJoinRelation relation)
        {
            var left = BuildRelation(relation.Left);
            var right = BuildRelation(relation.Right);
            var valueSlotMapping = BuildValueSlotMapping(relation);
            var predicate = BuildPredicate(relation.Condition, valueSlotMapping);
            return new InnerNestedLoopsIterator(left, right, predicate);
        }

        private Iterator BuildHashMatch(BoundHashMatchRelation relation)
        {
            var build = BuildRelation(relation.Build);
            var buildIndex = BuildValueSlotMapping(relation.Build)[relation.BuildKey];
            var probe = BuildRelation(relation.Probe);
            var probeIndex = BuildValueSlotMapping(relation.Probe)[relation.ProbeKey];
            var valueSlotMapping = BuildValueSlotMapping(relation);
            var predicate = BuildPredicate(relation.Remainder, valueSlotMapping);
            return new HashMatchIterator(relation.LogicalOperator, build, probe, buildIndex, probeIndex, predicate);
        }

        private Iterator BuildFilter(BoundFilterRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var valueSlotMapping = BuildValueSlotMapping(relation.Input);
            var predicate = BuildPredicate(relation.Condition, valueSlotMapping);
            return new FilterIterator(input, predicate);
        }

        private Iterator BuildCompute(BoundComputeRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputValueSlotMapping = BuildValueSlotMapping(relation.Input);
            var definedValue = relation.DefinedValues
                                       .Select(dv => BuildFunction(dv.Expression, inputValueSlotMapping))
                                       .ToImmutableArray();
            return new ComputeScalarIterator(input, definedValue);
        }

        private Iterator BuildTop(BoundTopRelation relation)
        {
            return relation.TieEntries.Any()
                       ? BuildTopWithTies(relation)
                       : BuildTopWithoutTies(relation);
        }

        private Iterator BuildTopWithTies(BoundTopRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputValueSlotMapping = BuildValueSlotMapping(relation);
            var tieEntries = relation.TieEntries.Select(t => inputValueSlotMapping[t.ValueSlot]).ToImmutableArray();
            var tieComparers = relation.TieEntries.Select(t => t.Comparer).ToImmutableArray();
            return new TopWithTiesIterator(input, relation.Limit, tieEntries, tieComparers);
        }

        private Iterator BuildTopWithoutTies(BoundTopRelation relation)
        {
            var input = BuildRelation(relation.Input);
            return new TopIterator(input, relation.Limit);
        }

        private Iterator BuildSort(BoundSortRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputValueSlotMapping = BuildValueSlotMapping(relation);
            var sortEntries = relation.SortedValues
                                      .Select(v => inputValueSlotMapping[v.ValueSlot])
                                      .ToImmutableArray();
            var comparers = relation.SortedValues.Select(v => v.Comparer).ToImmutableArray();
            return relation.IsDistinct
                ? new DistinctSortIterator(input, sortEntries, comparers)
                : new SortIterator(input, sortEntries, comparers);
        }

        private Iterator BuildConcatenationRelation(BoundConcatenationRelation relation)
        {
            var inputs = relation.Inputs.Select(BuildRelation);
            var outputValueSlotCount = relation.DefinedValues.Length;
            return new ConcatenationIterator(inputs, outputValueSlotCount);
        }

        private Iterator BuildStreamAggregatesRelation(BoundStreamAggregatesRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputValueSlotMapping = BuildValueSlotMapping(relation.Input);
            var aggregators = relation.Aggregates
                                      .Select(a => a.Aggregatable.CreateAggregator())
                                      .ToImmutableArray();
            var argumentFunctions = relation.Aggregates
                                            .Select(a => BuildFunction(a.Argument, inputValueSlotMapping))
                                            .ToImmutableArray();
            var groupEntries = relation.Groups
                                       .Select(v => inputValueSlotMapping[v])
                                       .ToImmutableArray();
            return new StreamAggregateIterator(input, groupEntries, aggregators, argumentFunctions);
        }

        private Iterator BuildProject(BoundProjectRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputValueSlotMapping = BuildValueSlotMapping(relation.Input);
            var projectedIndices = relation.Outputs.Select(vs => inputValueSlotMapping[vs]).ToImmutableArray();
            return new ProjectionIterator(input, projectedIndices);
        }
    }
}