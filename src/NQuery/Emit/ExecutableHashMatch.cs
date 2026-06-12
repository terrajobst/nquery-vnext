#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.EmittedIterators;
using NQuery.Planning;

namespace NQuery.Emit
{
    // Hash join. The remainder predicate is compiled once, against the combined
    // (build ++ probe) slot layout, exactly the order of the HashMatchRowBuffer the
    // iterator feeds it. The build/probe key positions are resolved per execution from
    // the input iterators' row buffers.
    internal sealed class ExecutableHashMatch : ExecutableOperator
    {
        private static readonly EmittedPredicate AlwaysTrue = _ => true;

        private readonly ExecutableOperator _build;
        private readonly ExecutableOperator _probe;
        private readonly ValueSlot _buildKey;
        private readonly ValueSlot _probeKey;
        private readonly bool _preserveBuild;
        private readonly bool _preserveProbe;
        private readonly EmittedPredicate _remainder;

        public ExecutableHashMatch(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator build, ExecutableOperator probe, PhysicalHashMatchKind kind, ValueSlot buildKey, ValueSlot probeKey, ImmutableArray<LogicalExpression> remainder)
            : base(outputValueSlots)
        {
            _build = build;
            _probe = probe;
            _buildKey = buildKey;
            _probeKey = probeKey;
            _preserveBuild = kind is PhysicalHashMatchKind.LeftOuter or PhysicalHashMatchKind.FullOuter;
            _preserveProbe = kind is PhysicalHashMatchKind.FullOuter;

            var combined = build.OutputValueSlots.AddRange(probe.OutputValueSlots);
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(combined);
            _remainder = CompileConjunction(remainder, slotIndices);
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var build = _build.CreateIterator(outer);
            var probe = _probe.CreateIterator(outer);
            var buildIndex = _build.OutputValueSlots.IndexOf(_buildKey);
            var probeIndex = _probe.OutputValueSlots.IndexOf(_probeKey);
            return new EmittedHashMatchIterator(build, probe, buildIndex, probeIndex, _remainder, _preserveBuild, _preserveProbe);
        }

        // Each conjunct already yields false on NULL; an empty remainder means the hash
        // key alone decides the match.
        private static EmittedPredicate CompileConjunction(ImmutableArray<LogicalExpression> conditions, IReadOnlyDictionary<ValueSlot, int> slotIndices)
        {
            if (conditions.IsEmpty)
                return AlwaysTrue;

            var predicates = conditions
                             .Select(c => EmittedExpressionCompiler.CompilePredicate(c, slotIndices))
                             .ToImmutableArray();

            if (predicates.Length == 1)
                return predicates[0];

            return rowBuffer =>
            {
                foreach (var predicate in predicates)
                {
                    if (!predicate(rowBuffer))
                        return false;
                }

                return true;
            };
        }
    }
}
