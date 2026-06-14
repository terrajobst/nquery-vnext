using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;
using NQuery.Planning;

namespace NQuery.Emit;

// Hash join. The remainder predicate is compiled once, against the combined
// (build ++ probe) slot layout, exactly the order of the HashMatchRowBuffer the
// iterator feeds it. The build/probe key positions are resolved per execution from
// the input iterators' row buffers.
//
// When this hash match is the correlated part of an Apply's right side, its remainder
// can reference the ambient outer scope (e.g. a decorrelated EXISTS join whose residual
// still tests an outer column). It is then compiled against (outer ++ build ++ probe),
// and the iterator prepends the outer row buffer -- mirroring ExecutableFilter and
// ExecutableNestedLoops. outerSlots is empty iff no Apply encloses this join.
internal sealed class ExecutableHashMatch : ExecutableOperator
{
    private static readonly EmittedPredicate AlwaysTrue = _ => true;

    private readonly ExecutableOperator _build;
    private readonly ExecutableOperator _probe;
    private readonly ValueSlot _buildKey;
    private readonly ValueSlot _probeKey;
    private readonly bool _preserveBuild;
    private readonly bool _preserveProbe;
    private readonly bool _semi;
    private readonly bool _anti;
    private readonly bool _probing;
    private readonly EmittedPredicate _remainder;
    private readonly bool _correlated;

    public ExecutableHashMatch(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator build, ExecutableOperator probe, PhysicalHashMatchKind kind, ValueSlot buildKey, ValueSlot probeKey, ImmutableArray<LogicalExpression> remainder, ImmutableArray<ValueSlot> outerSlots, ValueSlot? probeColumn = null)
        : base(outputValueSlots)
    {
        _build = build;
        _probe = probe;
        _buildKey = buildKey;
        _probeKey = probeKey;
        _preserveBuild = kind is PhysicalHashMatchKind.LeftOuter or PhysicalHashMatchKind.FullOuter;
        _preserveProbe = kind is PhysicalHashMatchKind.FullOuter;
        _semi = kind is PhysicalHashMatchKind.LeftSemi;
        _anti = kind is PhysicalHashMatchKind.LeftAntiSemi;

        // A probing semi join (the decorrelated EXISTS) emits every build row with a
        // trailing boolean marker; the planner threads its slot through as probeColumn.
        _probing = probeColumn is not null;

        // The remainder sees both sides, so it is compiled against the combined
        // (build ++ probe) layout the iterator feeds it -- even for a semi/anti join,
        // whose output is build-only but whose match test still spans both inputs. When
        // correlated, the ambient outer scope is prepended (outer ++ build ++ probe), the
        // order the iterator's combined buffer feeds at run time.
        _correlated = !outerSlots.IsEmpty;
        var combined = build.OutputValueSlots.AddRange(probe.OutputValueSlots);
        var remainderSlots = _correlated ? outerSlots.AddRange(combined) : combined;
        var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(remainderSlots);
        _remainder = CompileConjunction(remainder, slotIndices);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        var build = _build.CreateIterator(outer);
        var probe = _probe.CreateIterator(outer);
        var buildIndex = _build.OutputValueSlots.IndexOf(_buildKey);
        var probeIndex = _probe.OutputValueSlots.IndexOf(_probeKey);

        // A correlated remainder reads the outer row; hand the outer buffer to the iterator
        // so it can prepend it (outer ++ build ++ probe) when evaluating the remainder.
        var remainderOuter = _correlated ? outer : null;
        return new EmittedHashMatchIterator(build, probe, buildIndex, probeIndex, _remainder, _preserveBuild, _preserveProbe, _semi, _anti, _probing, remainderOuter);
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
