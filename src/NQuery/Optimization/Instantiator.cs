using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal static class Instantiator
    {
        public static BoundRelation Instantiate(BoundRelation relation)
        {
            return Instantiate(relation, Enumerable.Empty<KeyValuePair<ValueSlot, ValueSlot>>());
        }

        public static BoundRelation Instantiate(BoundRelation relation, IEnumerable<KeyValuePair<ValueSlot, ValueSlot>> mapping)
        {
            var valueSlotRewriter = new ValueSlotRewriter(mapping);
            return valueSlotRewriter.RewriteRelation(relation);
        }

        private sealed class ValueSlotRewriter : BoundTreeRewriter
        {
            private readonly Dictionary<ValueSlot, ValueSlot> _valueSlotMapping = new();

            public ValueSlotRewriter(IEnumerable<KeyValuePair<ValueSlot, ValueSlot>> mapping)
            {
                foreach (var pair in mapping)
                    _valueSlotMapping.Add(pair.Key, pair.Value);
            }

            private void AddValueSlotMapping(IEnumerable<ValueSlot> valueSlots)
            {
                var unmappedSlots = valueSlots.Where(v => !_valueSlotMapping.ContainsKey(v));
                foreach (var unmappedSlot in unmappedSlots)
                {
                    var newSlot = unmappedSlot.Duplicate();
                    _valueSlotMapping.Add(unmappedSlot, newSlot);
                }
            }

            protected override ValueSlot RewriteValueSlot(ValueSlot valueSlot)
            {
                if (valueSlot is null)
                    return null;

                return _valueSlotMapping.TryGetValue(valueSlot, out var newSlot) ? newSlot : valueSlot;
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
                if (!node.DefinedValues.Any())
                    return node;

                var factory = node.DefinedValues.First().ValueSlot.Factory;
                var original = node.TableInstance;
                var valueSlotMapping = original.ColumnInstances.ToDictionary(c => c.Column, c => _valueSlotMapping[c.ValueSlot]);
                var instanceName = factory.CreateNamed(original.Name, typeof(int)).Name;
                var instance = new TableInstanceSymbol(instanceName, original.Table, (t, c) => valueSlotMapping[c]);
                var columnMapping = instance.ColumnInstances.ToDictionary(c => c.Column);
                var definedValues = node.DefinedValues.Select(d => columnMapping[d.Column]).ToImmutableArray();
                return node.Update(instance, definedValues);
            }
        }
    }
}