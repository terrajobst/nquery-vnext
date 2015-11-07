using System;

namespace NQuery.Binding
{
    internal sealed class ValueSlot
    {
        public ValueSlot(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public Type Type { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}