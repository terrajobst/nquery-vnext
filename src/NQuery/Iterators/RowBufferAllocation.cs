using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Iterators
{
    internal sealed class RowBufferAllocation
    {
        private readonly Dictionary<ValueSlot, int> _mapping;

        public static readonly RowBufferAllocation Empty = new RowBufferAllocation();

        private RowBufferAllocation()
        {
            _mapping = new Dictionary<ValueSlot, int>();
        }

        public RowBufferAllocation(RowBufferAllocation parent, RowBuffer rowBuffer, IEnumerable<ValueSlot> valueSlots)
        {
            Parent = parent;
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

        public RowBufferAllocation Parent { get; }

        public RowBuffer RowBuffer { get; }

        public RowBufferEntry this[ValueSlot valueSlot]
        {
            get
            {
                return !_mapping.ContainsKey(valueSlot) && Parent != null
                            ? Parent[valueSlot]
                            : new RowBufferEntry(RowBuffer, _mapping[valueSlot]);
            }
        }
    }
}