using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundHashMatchRelation : BoundRelation
    {
        private readonly BoundHashMatchOperator _logicalOperator;
        private readonly BoundRelation _build;
        private readonly BoundRelation _probe;
        private readonly ValueSlot _buildKey;
        private readonly ValueSlot _probeKey;
        private readonly BoundExpression _remainder;

        public BoundHashMatchRelation(BoundHashMatchOperator logicalOperator, BoundRelation build, BoundRelation probe, ValueSlot buildKey, ValueSlot probeKey, BoundExpression remainder)
        {
            _logicalOperator = logicalOperator;
            _build = build;
            _probe = probe;
            _buildKey = buildKey;
            _probeKey = probeKey;
            _remainder = remainder;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.HashMatchRelation; }
        }

        public BoundHashMatchOperator LogicalOperator
        {
            get { return _logicalOperator; }
        }

        public BoundRelation Build
        {
            get { return _build; }
        }

        public BoundRelation Probe
        {
            get { return _probe; }
        }

        public ValueSlot BuildKey
        {
            get { return _buildKey; }
        }

        public ValueSlot ProbeKey
        {
            get { return _probeKey; }
        }

        public BoundExpression Remainder
        {
            get { return _remainder; }
        }

        public BoundHashMatchRelation Update(BoundHashMatchOperator logicalOperator, BoundRelation build, BoundRelation probe, ValueSlot buildKey, ValueSlot probeKey, BoundExpression remainder)
        {
            if (logicalOperator == _logicalOperator && build == _build && probe == _probe && buildKey == _buildKey && probeKey == _probeKey && remainder == _remainder)
                return this;

            return new BoundHashMatchRelation(logicalOperator, build, probe, buildKey, probeKey, remainder);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _build.GetOutputValues().Concat(_probe.GetOutputValues());
        }
    }
}