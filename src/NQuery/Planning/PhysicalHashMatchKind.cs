#nullable enable

namespace NQuery.Planning
{
    // The join kinds a hash match can produce. Inner and left outer, and -- unlike
    // nested loops -- full outer, since a hash match sees all of both inputs in one
    // pass. Semi/anti joins are left to nested loops. Build is the join's left and probe
    // its right, so left outer keeps the build side and full outer keeps both.
    internal enum PhysicalHashMatchKind
    {
        Inner,
        LeftOuter,
        FullOuter
    }
}
