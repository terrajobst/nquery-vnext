using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class UnusedValueSlotRemover : BoundTreeRewriter
    {
        private ValueSlotRecorder _recorder;

        public override BoundRelation RewriteRelation(BoundRelation node)
        {
            if (_recorder is null)
            {
                _recorder = new ValueSlotRecorder();
                _recorder.Record(node.GetOutputValues());
            }

            return base.RewriteRelation(node);
        }

        private ImmutableArray<T> RemoveUnusedSlots<T>(ImmutableArray<T> array, Func<T, ValueSlot> valueSlotSelector)
        {
            return array.RemoveAll(v => !_recorder.UsedValueSlots.Contains(valueSlotSelector(v)));
        }

        protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            var definedValues = RemoveUnusedSlots(node.DefinedValues, d => d.ValueSlot);
            node = node.Update(node.TableInstance, definedValues);

            return base.RewriteTableRelation(node);
        }

        protected override BoundRelation RewriteFilterRelation(BoundFilterRelation node)
        {
            _recorder.Record(node.Condition);

            return base.RewriteFilterRelation(node);
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            if (node.Condition is not null)
                _recorder.Record(node.Condition);

            if (node.PassthruPredicate is not null)
                _recorder.Record(node.PassthruPredicate);

            if (node.Probe is not null)
                _recorder.Record(node.Probe);

            return base.RewriteJoinRelation(node);
        }

        protected override BoundRelation RewriteHashMatchRelation(BoundHashMatchRelation node)
        {
            _recorder.Record(node.BuildKey);
            _recorder.Record(node.ProbeKey);

            if (node.Remainder is not null)
                _recorder.Record(node.Remainder);

            return base.RewriteHashMatchRelation(node);
        }

        protected override BoundRelation RewriteComputeRelation(BoundComputeRelation node)
        {
            var definedValues = RemoveUnusedSlots(node.DefinedValues, d => d.ValueSlot);
            node = node.Update(node.Input, definedValues);

            _recorder.Record(definedValues);

            if (!definedValues.Any())
                return RewriteRelation(node.Input);

            return base.RewriteComputeRelation(node);
        }

        protected override BoundRelation RewriteGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            var aggregates = RemoveUnusedSlots(node.Aggregates, a => a.Output);
            node = node.Update(node.Input, node.Groups, aggregates);

            _recorder.Record(node.Aggregates);
            _recorder.Record(node.Groups);

            return base.RewriteGroupByAndAggregationRelation(node);
        }

        protected override BoundRelation RewriteStreamAggregatesRelation(BoundStreamAggregatesRelation node)
        {
            var aggregates = RemoveUnusedSlots(node.Aggregates, a => a.Output);
            node = node.Update(node.Input, node.Groups, aggregates);

            _recorder.Record(node.Aggregates);
            _recorder.Record(node.Groups);

            return base.RewriteStreamAggregatesRelation(node);
        }

        protected override BoundRelation RewriteUnionRelation(BoundUnionRelation node)
        {
            if (node.IsUnionAll)
            {
                var definedValues = RemoveUnusedSlots(node.DefinedValues, d => d.ValueSlot);
                node = node.Update(node.IsUnionAll, node.Inputs, definedValues, node.Comparers);
            }

            _recorder.Record(node.DefinedValues);

            return base.RewriteUnionRelation(node);
        }

        protected override BoundRelation RewriteConcatenationRelation(BoundConcatenationRelation node)
        {
            var definedValues = RemoveUnusedSlots(node.DefinedValues, d => d.ValueSlot);
            node = node.Update(node.Inputs, definedValues);

            _recorder.Record(node.DefinedValues);

            return base.RewriteConcatenationRelation(node);
        }

        protected override BoundRelation RewriteIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            _recorder.Record(node.Left.GetOutputValues());
            _recorder.Record(node.Right.GetOutputValues());

            return base.RewriteIntersectOrExceptRelation(node);
        }

        protected override BoundRelation RewriteProjectRelation(BoundProjectRelation node)
        {
            var outputs = RemoveUnusedSlots(node.Outputs, v => v);
            node = node.Update(node.Input, outputs);

            return base.RewriteProjectRelation(node);
        }

        protected override BoundRelation RewriteSortRelation(BoundSortRelation node)
        {
            _recorder.Record(node.SortedValues);

            return base.RewriteSortRelation(node);
        }

        protected override BoundRelation RewriteTopRelation(BoundTopRelation node)
        {
            _recorder.Record(node.TieEntries);

            return base.RewriteTopRelation(node);
        }

        protected override BoundRelation RewriteAssertRelation(BoundAssertRelation node)
        {
            _recorder.Record(node.Condition);

            return base.RewriteAssertRelation(node);
        }

        protected override BoundRelation RewriteTableSpoolPusher(BoundTableSpoolPusher node)
        {
            var inputs = node.Input.GetOutputValues().ToImmutableArray();
            _recorder.Record(inputs);

            return base.RewriteTableSpoolPusher(node);
        }

        protected override BoundExpression RewriteSingleRowSubselect(BoundSingleRowSubselect node)
        {
            _recorder.Record(node.Value);

            return base.RewriteSingleRowSubselect(node);
        }

        private sealed class ValueSlotRecorder
        {
            private readonly ValueSlotDependencyFinder _finder;

            public ValueSlotRecorder()
            {
                _finder = new ValueSlotDependencyFinder(UsedValueSlots);
            }

            public HashSet<ValueSlot> UsedValueSlots { get; } = new HashSet<ValueSlot>();

            public void Record(BoundExpression expression)
            {
                _finder.VisitExpression(expression);
            }

            public void Record(IEnumerable<ValueSlot> valueSlots)
            {
                UsedValueSlots.UnionWith(valueSlots);
            }

            public void Record(ValueSlot valueSlot)
            {
                UsedValueSlots.Add(valueSlot);
            }

            public void Record(ImmutableArray<BoundComputedValue> definedValues)
            {
                foreach (var definedValue in definedValues)
                {
                    UsedValueSlots.Add(definedValue.ValueSlot);
                    _finder.VisitExpression(definedValue.Expression);
                }
            }

            public void Record(ImmutableArray<BoundAggregatedValue> definedValues)
            {
                foreach (var definedValue in definedValues)
                {
                    UsedValueSlots.Add(definedValue.Output);
                    _finder.VisitExpression(definedValue.Argument);
                }
            }

            public void Record(ImmutableArray<BoundUnifiedValue> definedValues)
            {
                foreach (var definedValue in definedValues)
                {
                    UsedValueSlots.Add(definedValue.ValueSlot);
                    UsedValueSlots.UnionWith(definedValue.InputValueSlots);
                }
            }

            public void Record(ImmutableArray<BoundComparedValue> definedValues)
            {
                foreach (var definedValue in definedValues)
                    UsedValueSlots.Add(definedValue.ValueSlot);
            }
        }
    }
}