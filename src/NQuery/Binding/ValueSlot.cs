using System;

namespace NQuery.Binding
{
    internal sealed class ValueSlot
    {
        private readonly string _name;
        private readonly Type _type;

        public ValueSlot(string name, Type type)
        {
            _name = name;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return _name;
        }
    }
}