#nullable enable

using NQuery.AlgebraBinding;

namespace NQuery.Algebra
{
    // The sole variable leaf of the logical expression language: a reference to a
    // value slot. Both BoundValueSlotExpression and BoundColumnExpression (whose
    // symbol carries a slot) translate into this.
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
