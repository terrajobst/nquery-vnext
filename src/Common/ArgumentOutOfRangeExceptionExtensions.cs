#if NETFRAMEWORK

using System.Runtime.CompilerServices;

#nullable enable

namespace NQuery
{
    // .NET Framework lacks ArgumentOutOfRangeException.ThrowIfNegative/ThrowIfGreaterThan. These
    // mirror the .NET 8 helpers (int-only, which is all NQuery uses) as extension members so they
    // can be imported unqualified via GlobalUsings, keeping the bare ThrowIfNegative(...) call form
    // working on every target. (CallerArgumentExpressionAttribute is polyfilled in
    // ArgumentNullExceptionExtensions.cs and shared within the assembly.)
    internal static class ArgumentOutOfRangeExceptionExtensions
    {
        extension(ArgumentOutOfRangeException)
        {
            public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be a non-negative value.");
            }

            public static void ThrowIfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            {
                if (value > other)
                    throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be less than or equal to '{other}'.");
            }
        }
    }
}

#endif
