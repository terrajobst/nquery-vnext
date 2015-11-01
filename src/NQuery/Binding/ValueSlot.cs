using System;

namespace NQuery.Binding
{
    internal sealed class ValueSlot
    {
        private readonly string _formatString;
        private readonly int _number;

        public ValueSlot(ValueSlotFactory factory, string formatString, int number, Type type)
        {
            Factory = factory;
            _formatString = formatString;
            _number = number;
            Type = type;
        }

        public ValueSlotFactory Factory { get; }

        public string Name => string.Format(_formatString, _number);

        public Type Type { get; }

        public override string ToString()
        {
            return Name;
        }

        public ValueSlot Duplicate()
        {
            return Factory.Create(_formatString, Type);
        }
    }
}