#nullable enable

using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;

namespace NQuery.Refactor.Algebra
{
    internal sealed class LogicalUnion : LogicalOperator
    {
        public LogicalUnion(bool isUnionAll, ImmutableArray<LogicalOperator> inputs, ImmutableArray<BoundUnifiedValue> definedValues, ImmutableArray<IComparer> comparers)
        {
            IsUnionAll = isUnionAll;
            Inputs = inputs;
            DefinedValues = definedValues;
            Comparers = comparers;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Union;

        public bool IsUnionAll { get; }

        public ImmutableArray<LogicalOperator> Inputs { get; }

        public ImmutableArray<BoundUnifiedValue> DefinedValues { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToImmutableArray();
    }
}
