using System.Collections;

namespace NQuery.CodeAnalysis;

internal static class DefaultComparers
{
    // The engine's intrinsic comparer for a type when the catalog registers none. Strings compare
    // ordinally so ORDER BY / GROUP BY / DISTINCT line up with the =/<> operator and hash-join
    // equality (all ordinal); any other comparable type falls back to Comparer.Default, and an
    // uncomparable type has none. Both GlobalBinder.LookupComparer and LogicalOptimizer.ResolveComparer
    // route their fallback through here so the binder and the optimizer's domain grouping agree.
    public static IComparer? For(Type type)
    {
        if (type == typeof(string))
            return StringComparer.Ordinal;

        return type.IsComparable() ? Comparer.Default : null;
    }
}
