using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundHashMatchRelation : BoundRelation
    {
        public BoundHashMatchRelation(BoundHashMatchOperator logicalOperator, BoundRelation build, BoundRelation probe, ValueSlot buildKey, ValueSlot probeKey, BoundExpression remainder)
        {
            LogicalOperator = logicalOperator;
            Build = build;
            Probe = probe;
            BuildKey = buildKey;
            ProbeKey = probeKey;
            Remainder = remainder;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.HashMatchRelation; }
        }

        public BoundHashMatchOperator LogicalOperator { get; }

        public BoundRelation Build { get; }

        public BoundRelation Probe { get; }

        public ValueSlot BuildKey { get; }

        public ValueSlot ProbeKey { get; }

        public BoundExpression Remainder { get; }

        public BoundHashMatchRelation Update(BoundHashMatchOperator logicalOperator, BoundRelation build, BoundRelation probe, ValueSlot buildKey, ValueSlot probeKey, BoundExpression remainder)
        {
            if (logicalOperator == LogicalOperator && build == Build && probe == Probe && buildKey == BuildKey && probeKey == ProbeKey && remainder == Remainder)
                return this;

            return new BoundHashMatchRelation(logicalOperator, build, probe, buildKey, probeKey, remainder);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Build.GetDefinedValues().Concat(Probe.GetDefinedValues());
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Build.GetOutputValues().Concat(Probe.GetOutputValues());
        }
    }
}