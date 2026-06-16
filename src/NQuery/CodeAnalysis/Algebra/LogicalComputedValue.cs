namespace NQuery.CodeAnalysis.Algebra;

// The logical counterpart of BoundComputedValue: a computed expression bound
// to the value slot it defines.
internal sealed class LogicalComputedValue
{
    public LogicalComputedValue(LogicalExpression expression, ValueSlot valueSlot)
    {
        Expression = expression;
        ValueSlot = valueSlot;
    }

    public LogicalExpression Expression { get; }

    public ValueSlot ValueSlot { get; }
}
