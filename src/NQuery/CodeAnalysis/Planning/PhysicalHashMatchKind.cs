namespace NQuery.CodeAnalysis.Planning;

// The join kinds a hash match can produce. Inner and left outer, and -- unlike
// nested loops -- full outer, since a hash match sees all of both inputs in one
// pass. It also covers left semi / left anti-semi: the build side is hashed and each
// build row is emitted (semi) or suppressed (anti) by whether the probe found a
// match, decided in a single pass. Build is the join's left and probe its right, so
// left outer keeps the build side, full outer keeps both, and semi/anti output the
// build side only.
internal enum PhysicalHashMatchKind
{
    Inner,
    LeftOuter,
    FullOuter,
    LeftSemi,
    LeftAntiSemi
}
