using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Planning;

namespace NQuery.CodeAnalysis.Emit;

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
    private static readonly CompiledPredicate AlwaysTrue = _ => true;

    private readonly ExecutableOperator _build;
    private readonly ExecutableOperator _probe;
    private readonly ValueSlot _buildKey;
    private readonly ValueSlot _probeKey;
    private readonly bool _preserveBuild;
    private readonly bool _preserveProbe;
    private readonly bool _semi;
    private readonly bool _anti;
    private readonly bool _probing;
    private readonly CompiledPredicate _remainder;
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
        var slotIndices = ExpressionCompiler.CreateSlotIndices(remainderSlots);
        _remainder = CompileConjunction(remainder, slotIndices);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        var build = _build.CreateIterator(outer);
        var probe = _probe.CreateIterator(outer);

        // The join key on each side is read boxed (it becomes the hash-table key), so it
        // is resolved to an entry over that side's row buffer.
        var buildKey = new RowBufferAllocation(null, build.RowBuffer, _build.OutputValueSlots)[_buildKey];
        var probeKey = new RowBufferAllocation(null, probe.RowBuffer, _probe.OutputValueSlots)[_probeKey];

        // A correlated remainder reads the outer row; hand the outer buffer to the iterator
        // so it can prepend it (outer ++ build ++ probe) when evaluating the remainder.
        var remainderOuter = _correlated ? outer : null;
        return new HashMatchIterator(build, probe, buildKey, probeKey, _remainder, _preserveBuild, _preserveProbe, _semi, _anti, _probing, remainderOuter);
    }

    // Each conjunct already yields false on NULL; an empty remainder means the hash
    // key alone decides the match.
    private static CompiledPredicate CompileConjunction(ImmutableArray<LogicalExpression> conditions, FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices)
    {
        if (conditions.IsEmpty)
            return AlwaysTrue;

        var predicates = conditions
                         .Select(c => ExpressionCompiler.CompilePredicate(c, slotIndices))
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
