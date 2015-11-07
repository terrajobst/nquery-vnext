using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Iterators
{
    internal sealed class RowBufferAllocation
    {
        private readonly Dictionary<ValueSlot, int> _mapping;

        public RowBufferAllocation(RowBuffer rowBuffer, IEnumerable<ValueSlot> valueSlots)
        {
            RowBuffer = rowBuffer;
            _mapping = GetValueMapping(valueSlots.ToImmutableArray());
        }

        private static Dictionary<ValueSlot, int> GetValueMapping(ImmutableArray<ValueSlot> valueSlots)
        {
            var dictionary = new Dictionary<ValueSlot, int>(valueSlots.Length);
            for (var i = 0; i < valueSlots.Length; i++)
            {
                var outputValue = valueSlots[i];
                if (!dictionary.ContainsKey(outputValue))
                    dictionary[outputValue] = i;
            }

            return dictionary;
        }

        public bool TryGetValueSlot(ValueSlot valueSlot, out int index)
        {
            return _mapping.TryGetValue(valueSlot, out index);
        }

        public int this[ValueSlot valueSlot]
        {
            get { return _mapping[valueSlot]; }
        }

        public RowBuffer RowBuffer { get; }
    }
}