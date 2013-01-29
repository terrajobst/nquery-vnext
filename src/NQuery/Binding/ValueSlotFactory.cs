using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class ValueSlotFactory
    {
        private readonly Dictionary<string, int> _usedNames = new Dictionary<string, int>();
        private int _nextTemporaryNumber = 1000;

        public ValueSlot CreateTemporaryValueSlot(Type type)
        {
            var name = string.Format("Expr{0}", _nextTemporaryNumber);
            _nextTemporaryNumber++;
            return new ValueSlot(name, type);
        }

        public ValueSlot CreateValueSlot(string name, Type type)
        {
            int highestNumber;
            _usedNames.TryGetValue(name, out highestNumber);

            highestNumber++;
            _usedNames[name] = highestNumber;

            var qualifiedName = name + ":" + highestNumber;
            return new ValueSlot(qualifiedName, type);
        }
    }
}