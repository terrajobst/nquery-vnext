using System;
using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Iterators
{
    internal sealed class ValueSlotSettings
    {
        private readonly IReadOnlyDictionary<ValueSlot, int> _valueSlotToRowBufferIndex;
        private readonly Func<RowBuffer> _rowBufferProvider;

        public ValueSlotSettings(IReadOnlyDictionary<ValueSlot, int> valueSlotToRowBufferIndex, Func<RowBuffer> rowBufferProvider)
        {
            _valueSlotToRowBufferIndex = valueSlotToRowBufferIndex;
            _rowBufferProvider = rowBufferProvider;
        }

        public int GetRowBufferIndex(ValueSlot valueSlot)
        {
            return _valueSlotToRowBufferIndex[valueSlot];
        }

        public Func<RowBuffer> RowBufferProvider
        {
            get { return _rowBufferProvider; }
        }
    }
}