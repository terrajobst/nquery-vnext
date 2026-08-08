using System.Globalization;

namespace NQuery.Tests.Oracle;

// Cross-engine row comparison for the SQLite oracle tests.
//
// Two engines that agree on relational semantics can still disagree on (a) representation -- one
// returns Int32 where the other returns Int64 -- and (b) result *order*, whenever ORDER BY doesn't
// fully determine it (and even then, on where NULLs sort). Neither is a correctness defect, so the
// comparison normalizes representation, and the caller picks how order is treated:
//
//   * AssertEqualUnordered -- multiset comparison, for the common case where only the set of rows
//     matters and ORDER BY doesn't fully determine the order.
//   * AssertEqualOrdered   -- positional comparison, for queries whose ORDER BY fully determines
//     the order and where ordering itself is what's under test.
internal static class RowSet
{
    // Canonicalizes one cell so the same logical value from either engine compares equal:
    //   null / DBNull        -> null
    //   bool                 -> 0 / 1   (SQLite has no boolean; it surfaces as an integer)
    //   any integral type    -> long
    //   float / double       -> double
    //   everything else (string, decimal, DateTime, byte[]) is left as-is and compared by Signature.
    public static object? Normalize(object? value)
    {
        if (value is null or DBNull)
            return null;

        return value switch
        {
            bool b => b ? 1L : 0L,
            sbyte or byte or short or ushort or int or uint or long or ulong =>
                Convert.ToInt64(value, CultureInfo.InvariantCulture),
            float or double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            _ => value,
        };
    }

    // Multiset comparison: same rows, same multiplicities, any order. Use this for queries without
    // a fully-determined ORDER BY (the common case), so the test doesn't depend on either engine's
    // ordering of otherwise-unordered rows. Sorting the signatures (rather than set-comparing) keeps
    // duplicates significant, so a join that drops or doubles a row is still caught.
    public static void AssertEqualUnordered(IReadOnlyList<object[]> expected, IReadOnlyList<object[]> actual)
    {
        var expectedSignatures = expected.Select(RowSignature).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var actualSignatures = actual.Select(RowSignature).OrderBy(s => s, StringComparer.Ordinal).ToList();

        Assert.Equal(expectedSignatures, actualSignatures);
    }

    // Positional comparison: rows must match in the order each engine returned them. Use this only
    // for queries whose ORDER BY fully determines the order; otherwise it depends on implementation-
    // defined ordering (tie-breaking, NULL placement, string collation) and will report false
    // failures.
    public static void AssertEqualOrdered(IReadOnlyList<object[]> expected, IReadOnlyList<object[]> actual)
    {
        var expectedSignatures = expected.Select(RowSignature).ToList();
        var actualSignatures = actual.Select(RowSignature).ToList();

        Assert.Equal(expectedSignatures, actualSignatures);
    }

    private static string RowSignature(object[] row)
    {
        return string.Join(" | ", row.Select(CellSignature));
    }

    private static string CellSignature(object value)
    {
        return value switch
        {
            null => "<null>",
            long l => $"i:{l.ToString(CultureInfo.InvariantCulture)}",
            // Round before formatting so representational noise between engines doesn't diverge.
            double d => $"f:{Math.Round(d, 10).ToString("R", CultureInfo.InvariantCulture)}",
            decimal m => $"m:{m.ToString(CultureInfo.InvariantCulture)}",
            string s => $"s:{s}",
            DateTime dt => $"d:{dt.ToString("o", CultureInfo.InvariantCulture)}",
            _ => $"?:{Convert.ToString(value, CultureInfo.InvariantCulture)}",
        };
    }
}
