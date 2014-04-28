using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal sealed class CommonTableExpressionInstantiator : BoundTreeRewriter
    {
        private readonly Dictionary<ValueSlot, ValueSlot> _valueSlotMapping = new Dictionary<ValueSlot, ValueSlot>();
        private int _cteCount;

        protected override ValueSlot RewriteValueSlot(ValueSlot valueSlot)
        {
            if (valueSlot == null)
                return null;

            ValueSlot newSlot;
            return _valueSlotMapping.TryGetValue(valueSlot, out newSlot) ? newSlot : valueSlot;
        }

        protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            var symbol = node.TableInstance.Table as CommonTableExpressionSymbol;
            if (symbol == null)
                return base.RewriteTableRelation(node);

            var prototype = symbol.Query.Relation;
            var instantiatedQuery = InstantiateCommonTableExpression(prototype);

            AddOutputSlotMapping(node.TableInstance, instantiatedQuery);

            // For regular SELECT queries the output node will be a projection.
            // We don't need this moving forward so it's safe to omit.
            //
            // NOTE: We need to keep this after we added the output slot mapping.
            //       Otherwise the slots will not align.

            var projection = instantiatedQuery as BoundProjectRelation;
            var result = projection == null ? instantiatedQuery : projection.Input;


            return result;
        }

        private void AddOutputSlotMapping(TableInstanceSymbol commonTableExpressionReference, BoundRelation instantiatedCommonTableExpression)
        {
            var originalSlots = commonTableExpressionReference.ColumnInstances.Select(c => c.ValueSlot).ToImmutableArray();
            var instanceSlots = instantiatedCommonTableExpression.GetOutputValues().ToImmutableArray();

            for (var i = 0; i < originalSlots.Length; i++)
            {
                var originalSlot = originalSlots[i];
                var instanceSlot = instanceSlots[i];
                _valueSlotMapping.Add(originalSlot, instanceSlot);
            }
        }

        private BoundRelation InstantiateCommonTableExpression(BoundRelation relation)
        {
            _cteCount++;
            var valueSlotRewriter = new ValueSlotRewriter(CreateNewName);
            return valueSlotRewriter.RewriteRelation(relation);
        }

        private string CreateNewName(string name)
        {
            return string.Format("{0}:CTE:{1}", name, _cteCount);
        }

        private sealed class ValueSlotRewriter : BoundTreeRewriter
        {
            private readonly Func<string, string> _nameProvider;
            private readonly Dictionary<ValueSlot, ValueSlot> _valueSlotMapping = new Dictionary<ValueSlot, ValueSlot>();

            public ValueSlotRewriter(Func<string, string> nameProvider)
            {
                _nameProvider = nameProvider;
            }

            private void AddValueSlotMapping(IEnumerable<ValueSlot> valueSlots)
            {
                foreach (var valueSlot in valueSlots)
                {
                    var name = _nameProvider(valueSlot.Name);
                    var newSlot = new ValueSlot(name, valueSlot.Type);
                    _valueSlotMapping.Add(valueSlot, newSlot);
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