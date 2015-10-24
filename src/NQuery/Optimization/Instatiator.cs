using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal static class Instatiator
    {
        public static BoundRelation Instantiate(BoundRelation relation, Func<string, string> valueSlotNameProvider)
        {
            return Instantiate(relation, valueSlotNameProvider, Enumerable.Empty<KeyValuePair<ValueSlot, ValueSlot>>());
        }

        public static BoundRelation Instantiate(BoundRelation relation, Func<string, string> valueSlotNameProvider, IEnumerable<KeyValuePair<ValueSlot, ValueSlot>> mapping)
        {
            var valueSlotRewriter = new ValueSlotRewriter(mapping, valueSlotNameProvider);
            return valueSlotRewriter.RewriteRelation(relation);
        }

        private sealed class ValueSlotRewriter : BoundTreeRewriter
        {
            private readonly Func<string, string> _nameProvider;
            private readonly Dictionary<ValueSlot, ValueSlot> _valueSlotMapping = new Dictionary<ValueSlot, ValueSlot>();

            public ValueSlotRewriter(IEnumerable<KeyValuePair<ValueSlot, ValueSlot>> mapping, Func<string, string> nameProvider)
            {
                _nameProvider = nameProvider;
                foreach (var pair in mapping)
                    _valueSlotMapping.Add(pair.Key, pair.Value);
            }

            private void AddValueSlotMapping(IEnumerable<ValueSlot> valueSlots)
            {
                var unmappedSlots = valueSlots.Where(v => !_valueSlotMapping.ContainsKey(v));
                foreach (var unmappedSlot in unmappedSlots)
                {
                    var name = _nameProvider(unmappedSlot.Name);
                    var newSlot = new ValueSlot(name, unmappedSlot.Type);
                    _valueSlotMapping.Add(unmappedSlot, newSlot);
                }
            }

            protected override ValueSlot RewriteValueSlot(ValueSlot valueSlot)
            {
                if (valueSlot == null)
                    return null;

                ValueSlot newSlot;
                return _valueSlotMapping.TryGetValue(valueSlot, out newSlot) ? newSlot : valueSlot;
            }

            public override BoundRelation RewriteRelation(BoundRelation node)
            {
                AddValueSlotMapping(node.GetDefinedValues());
                return base.RewriteRelation(node);
            }

            protected override BoundExpression RewriteSingleRowSubselect(BoundSingleRowSubselect node)
            {
                var input = RewriteRelation(node.Relation);
                var valueSlot = RewriteValueSlot(node.Value);
                return node.Update(valueSlot, input);
            }

            protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
            {
                var original = node.TableInstance;
                var valueSlotMapping = original.ColumnInstances.ToDictionary(c => c.Column, c => _valueSlotMapping[c.ValueSlot]);
                var instanceName = _nameProvider(original.Name);
                var instance = new TableInstanceSymbol(instanceName, original.Table, (t, c) => valueSlotMapping[c]);
                var columnMapping = instance.ColumnInstances.ToDictionary(c => c.Column);
                var definedValues = node.DefinedValues.Select(d => columnMapping[d.Column]).ToImmutableArray();
                return node.Update(instance, definedValues);
            }
        }
    }
}