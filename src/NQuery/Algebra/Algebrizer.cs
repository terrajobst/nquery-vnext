using System;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    internal sealed partial class Algebrizer
    {
        private int _producedSlots;

        private Algebrizer()
        {
        }

        public static AlgebraNode Algebrize(BoundQuery node)
        {
            var algebrizer = new Algebrizer();
            return algebrizer.AlgebrizeQuery(node);
        }

        private ValueSlot CreateValueSlot(Type type)
        {
            var slotNumber = 1000 + _producedSlots;
            var displayName = string.Format("Temp{0}", slotNumber);
            _producedSlots++;

            return new ValueSlot(displayName, type);
        }
    }
}