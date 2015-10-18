using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal sealed class CommonTableExpressionInstantiator : BoundTreeRewriter
    {
        private int _cteCount;

        protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            var symbol = node.TableInstance.Table as CommonTableExpressionSymbol;
            if (symbol == null)
                return base.RewriteTableRelation(node);

            var prototype = symbol.Query.Relation;
            var instantiatedQuery = InstantiateCommonTableExpression(node.GetOutputValues(), prototype);

            // For regular SELECT queries the output node will be a projection.
            // We don't need this moving forward so it's safe to omit.
            //
            // NOTE: We need to keep this after we added the output slot mapping.
            //       Otherwise the slots will not align.

            var projection = instantiatedQuery as BoundProjectRelation;
            var result = projection == null ? instantiatedQuery : projection.Input;

            return result;
        }

        private BoundRelation InstantiateCommonTableExpression(IEnumerable<ValueSlot> outputValues, BoundRelation relation)
        {
            var mapping = outputValues.Zip(relation.GetOutputValues(), (v, k) => new KeyValuePair<ValueSlot, ValueSlot>(k, v));

            _cteCount++;
            var valueSlotRewriter = new ValueSlotRewriter(mapping, CreateNewName);
            return valueSlotRewriter.RewriteRelation(relation);
        }

        private string CreateNewName(string name)
        {
            return $"{name}:CTE:{_cteCount}";
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

            protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
            {
                var original = node.TableInstance;
                var valueSlotMapping = original.ColumnInstances.ToDictionary(c => c.Column, c => _valueSlotMapping[c.ValueSlot]);
                var instanceName = _nameProvider(original.Name);
                var instance = new TableInstanceSymbol(instanceName, original.Table, (t, c) => valueSlotMapping[c]);
                return node.Update(instance);
            }
        }
    }
}