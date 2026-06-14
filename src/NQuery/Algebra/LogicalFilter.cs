using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.Algebra;

// Conditions are an implicit conjunction (AND of all); an empty list means
// "true". Storing the predicate as a list of conjuncts -- rather than a single
// AND-tree -- is what lets selection pushdown move individual conjuncts to the
// side of a join that defines their slots.
internal sealed class LogicalFilter : LogicalOperator
{
    public LogicalFilter(LogicalOperator input, ImmutableArray<LogicalExpression> conditions)
    {
        Input = input;
        Conditions = conditions;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Filter;

    public LogicalOperator Input { get; }

    public ImmutableArray<LogicalExpression> Conditions { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
