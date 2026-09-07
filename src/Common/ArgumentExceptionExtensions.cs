#if NETFRAMEWORK

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NQuery;

// .NET Framework lacks ArgumentException.ThrowIfNullOrEmpty. This mirrors the .NET 8 helper,
// including its split between the two exception types, as an extension member so it can be
// imported unqualified via GlobalUsings, keeping the bare ThrowIfNullOrEmpty(...) call form
// working on every target. (NotNullAttribute and CallerArgumentExpressionAttribute are
// polyfilled in ArgumentNullExceptionExtensions.cs and shared within the assembly.)
internal static class ArgumentExceptionExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
                Throw(argument, paramName);
        }
    }

    // Not an extension member, unlike ArgumentNullException.Throw: the BCL has no such method on
    // ArgumentException, so adding one would put a second unqualified Throw into every file that
    // imports these.
    [DoesNotReturn]
    private static void Throw(string? argument, string? paramName)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);

        throw new ArgumentException(@"The value cannot be an empty string.", paramName);
    }
}

#endif
