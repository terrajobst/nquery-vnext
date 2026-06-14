#nullable enable

namespace NQuery.Algebra
{
    // The sole variable leaf of the logical expression language: a reference to a
    // value slot. Both BoundValueExpression and BoundColumnExpression translate into
    // this -- the algebrizer maps their IBoundValue to the slot referenced here.
    internal sealed class LogicalValueSlotExpression : LogicalExpression
    {
        public LogicalValueSlotExpression(ValueSlot valueSlot)
        {
            ValueSlot = valueSlot;
        }

        public override LogicalExpressionKind Kind => LogicalExpressionKind.ValueSlot;

        public override Type Type => ValueSlot.Type;

        public ValueSlot ValueSlot { get; }
    }
}
