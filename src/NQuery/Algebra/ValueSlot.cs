using System;

namespace NQuery.Algebra
{
    internal sealed class ValueSlot
    {
        private readonly string _displayName;
        private readonly Type _type;

        public ValueSlot(string displayName, Type type)
        {
            _displayName = displayName;
            _type = type;
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}