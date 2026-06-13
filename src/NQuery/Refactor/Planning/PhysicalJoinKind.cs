#nullable enable

namespace NQuery.Refactor.Planning
{
    // The join kinds a nested-loops join can physically produce. This is a subset of
    // the logical LogicalJoinKind: FullOuter is absent, because nested loops cannot
    // produce a full outer join (it needs a hash match). The planner is therefore
    // forced to decide what to do with a full-outer logical join when it lowers one,
    // rather than letting it reach the executor and throw -- the impossible state is
    // simply unrepresentable here. A future PhysicalHashMatch will carry its own kind
    // set that does include FullOuter.
    internal enum PhysicalJoinKind
    {
        Inner,
        LeftOuter,
        LeftSemi,
        LeftAntiSemi
    }
}
