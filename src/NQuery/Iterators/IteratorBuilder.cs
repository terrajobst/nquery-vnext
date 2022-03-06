using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Iterators
{
    internal sealed class IteratorBuilder
    {
        private readonly Stack<TableSpoolStack> _tableSpoolStack = new();

        private RowBufferAllocation _outerRowBufferAllocation;

        public static Iterator Build(BoundRelation relation)
        {
            var builder = new IteratorBuilder();
            return builder.BuildRelation(relation);
        }

        private RowBufferAllocation BuildRowBufferAllocation(BoundRelation input, RowBuffer rowBuffer)
        {
            return new RowBufferAllocation(_outerRowBufferAllocation, rowBuffer, input.GetOutputValues());
        }

        private static IteratorFunction BuildFunction(BoundExpression expression, RowBufferAllocation allocation)
        {
            return ExpressionBuilder.BuildIteratorFunction(expression, allocation);
        }

        private static IteratorPredicate BuildPredicate(BoundExpression predicate, bool nullValue, RowBufferAllocation allocation)
        {
            return ExpressionBuilder.BuildIteratorPredicate(predicate, nullValue, allocation);
        }

        private Iterator BuildRelation(BoundRelation relation)
        {
            switch (relation.Kind)
            {
                case BoundNodeKind.EmptyRelation:
                    return BuildEmpty();
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
                case BoundNodeKind.AssertRelation:
                    return BuildAssert((BoundAssertRelation)relation);
                case BoundNodeKind.TableSpoolPusher:
                    return BuildTableSpoolPusher((BoundTableSpoolPusher)relation);
                case BoundNodeKind.TableSpoolPopper:
                    return BuildTableSpoolPopper((BoundTableSpoolPopper)relation);
                default:
                    throw ExceptionBuilder.UnexpectedValue(relation.Kind);
            }
        }

        private static Iterator BuildEmpty()
        {
            return new EmptyIterator();
        }

        private static Iterator BuildConstant()
        {
            return new ConstantIterator();
        }

        private static Iterator BuildTable(BoundTableRelation relation)
        {
            var schemaTableSymbol = (SchemaTableSymbol)relation.TableInstance.Table;
            var tableDefinition = schemaTableSymbol.Definition;
            var columnInstances = relation.DefinedValues;
            var definedValues = columnInstances.Select(ci => ci.Column)
                                               .Cast<SchemaColumnSymbol>()
                                               .Select(c => BuildColumnAccess(c.Definition));
            return new TableIterator(tableDefinition, definedValues);
        }

        private static Func<object, object> BuildColumnAccess(ColumnDefinition definition)
        {
            var instance = Expression.Parameter(typeof(object));
            var body = definition.CreateInvocation(instance);
            var lambda = Expression.Lambda<Func<object, object>>(body, instance);
            return lambda.Compile();
        }

        private Iterator BuildJoin(BoundJoinRelation relation)
        {
            var left = BuildRelation(relation.Left);
            _outerRowBufferAllocation = BuildRowBufferAllocation(relation.Left, left.RowBuffer);

            var right = BuildRelation(relation.Right);
            var combinedAllocation = BuildRowBufferAllocation(relation.Right, right.RowBuffer);
            _outerRowBufferAllocation = _outerRowBufferAllocation.Parent;

            var predicate = BuildPredicate(relation.Condition, true, combinedAllocation);

            var passthruPredicate = BuildPredicate(relation.PassthruPredicate, false, combinedAllocation);

            switch (relation.JoinType)
            {
                case BoundJoinType.Inner:
                    Debug.Assert(relation.Probe is null);
                    return new InnerNestedLoopsIterator(left, right, predicate, passthruPredicate);
                case BoundJoinType.LeftSemi:
                    return relation.Probe is null
                        ? new LeftSemiNestedLoopsIterator(left, right, predicate, passthruPredicate)
                        : new ProbingLeftSemiNestedLoopsIterator(left, right, predicate);
                case BoundJoinType.LeftAntiSemi:
                    Debug.Assert(relation.Probe is null);
                    return new LeftAntiSemiNestedLoopsIterator(left, right, predicate, passthruPredicate);
                case BoundJoinType.LeftOuter:
                    return new LeftOuterNestedLoopsIterator(left, right, predicate, passthruPredicate);
                default:
                    throw ExceptionBuilder.UnexpectedValue(relation.JoinType);
            }
        }

        private Iterator BuildHashMatch(BoundHashMatchRelation relation)
        {
            var build = BuildRelation(relation.Build);
            var buildAllocation = BuildRowBufferAllocation(relation.Build, build.RowBuffer);
            var buildEntry = buildAllocation[relation.BuildKey];

            var probe = BuildRelation(relation.Probe);
            var probeAllocation = BuildRowBufferAllocation(relation.Probe, probe.RowBuffer);
            var probeEntry = probeAllocation[relation.ProbeKey];

            var outputRowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
            var outputAllocation = BuildRowBufferAllocation(relation, outputRowBuffer);
            var predicate = BuildPredicate(relation.Remainder, true, outputAllocation);

            Debug.Assert(buildEntry.RowBuffer == build.RowBuffer);
            Debug.Assert(probeEntry.RowBuffer == probe.RowBuffer);

            return new HashMatchIterator(relation.LogicalOperator, build, probe, buildEntry.Index, probeEntry.Index, predicate, outputRowBuffer);
        }

        private Iterator BuildFilter(BoundFilterRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var allocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var predicate = BuildPredicate(relation.Condition, true, allocation);
            return new FilterIterator(input, predicate);
        }

        private Iterator BuildCompute(BoundComputeRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var allocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var definedValue = relation.DefinedValues
                                       .Select(dv => BuildFunction(dv.Expression, allocation))
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
            var rowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var tieEntries = relation.TieEntries.Select(t => rowBufferAllocation[t.ValueSlot]).ToImmutableArray();
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
            var inputRowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var sortEntries = relation.SortedValues
                                      .Select(v => inputRowBufferAllocation[v.ValueSlot])
                                      .ToImmutableArray();
            var comparers = relation.SortedValues.Select(v => v.Comparer).ToImmutableArray();
            return relation.IsDistinct
                ? new DistinctSortIterator(input, sortEntries, comparers)
                : new SortIterator(input, sortEntries, comparers);
        }

        private Iterator BuildConcatenationRelation(BoundConcatenationRelation relation)
        {
            var inputs = relation.Inputs.Select(BuildRelation).ToImmutableArray();
            var inputEntries = from i in Enumerable.Range(0, inputs.Length)
                               let allocation = BuildRowBufferAllocation(relation.Inputs[i], inputs[i].RowBuffer)
                               let slots = relation.DefinedValues.Select(d => d.InputValueSlots[i])
                               let entries = slots.Select(s => allocation[s])
                               select entries.ToImmutableArray();

            return new ConcatenationIterator(inputs, inputEntries);
        }

        private Iterator BuildStreamAggregatesRelation(BoundStreamAggregatesRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var allocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var aggregators = relation.Aggregates
                                      .Select(a => a.Aggregatable.CreateAggregator())
                                      .ToImmutableArray();
            var argumentFunctions = relation.Aggregates
                                            .Select(a => BuildFunction(a.Argument, allocation))
                                            .ToImmutableArray();
            var groupEntries = relation.Groups
                                       .Select(g => allocation[g.ValueSlot])
                                       .ToImmutableArray();
            var comparers = relation.Groups.Select(v => v.Comparer).ToImmutableArray();
            return new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions);
        }

        private Iterator BuildProject(BoundProjectRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var inputRowBufferAllocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var projectedIndices = relation.Outputs.Select(vs => inputRowBufferAllocation[vs]).ToImmutableArray();
            return new ProjectionIterator(input, projectedIndices);
        }

        private Iterator BuildAssert(BoundAssertRelation relation)
        {
            var input = BuildRelation(relation.Input);
            var allocation = BuildRowBufferAllocation(relation.Input, input.RowBuffer);
            var predicate = BuildPredicate(relation.Condition, true, allocation);
            return new AssertIterator(input, predicate, relation.Message);
        }

        private Iterator BuildTableSpoolPusher(BoundTableSpoolPusher relation)
        {
            var stack = new TableSpoolStack(relation.Input.GetOutputValues().Count());
            _tableSpoolStack.Push(stack);
            var input = BuildRelation(relation.Input);
            _tableSpoolStack.Pop();

            return new TableSpoolIterator(input, stack);
        }

        private Iterator BuildTableSpoolPopper(BoundTableSpoolPopper relation)
        {
            var stack = _tableSpoolStack.Peek();
            return new TableSpoolRefIterator(stack);
        }
    }
}