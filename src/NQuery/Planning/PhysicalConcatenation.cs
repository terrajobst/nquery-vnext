#nullable enable

using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Planning
{
    internal sealed class PhysicalConcatenation : PhysicalOperator
    {
        public PhysicalConcatenation(bool isUnionAll, ImmutableArray<PhysicalOperator> inputs, ImmutableArray<BoundUnifiedValue> definedValues, ImmutableArray<IComparer> comparers)
        {
            IsUnionAll = isUnionAll;
            Inputs = inputs;
            DefinedValues = definedValues;
            Comparers = comparers;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Concatenation;

        public bool IsUnionAll { get; }

        public ImmutableArray<PhysicalOperator> Inputs { get; }

        public ImmutableArray<BoundUnifiedValue> DefinedValues { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToImmutableArray();
    }
}
