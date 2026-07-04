using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace NQuery;

// Extends the ThrowIfNull guard family to ImmutableArray<T>. The BCL's ArgumentNullException.ThrowIfNull
// takes object?, so a defaulted ImmutableArray (a struct that boxes to a non-null reference) slips
// through it; this overload throws for the uninitialized/default array -- the ImmutableArray equivalent
// of null. Imported unqualified via GlobalUsings so call sites keep the bare ThrowIfNull(...) form.
internal static class ArgumentValidation
{
    public static void ThrowIfNull<T>(ImmutableArray<T> argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument.IsDefault)
            throw new ArgumentNullException(paramName);
    }
}
