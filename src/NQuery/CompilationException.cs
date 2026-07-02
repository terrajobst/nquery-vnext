using System.Collections.Immutable;

using NQuery.CodeAnalysis;

namespace NQuery;

public sealed class CompilationException : Exception
{
    public CompilationException(IReadOnlyCollection<Diagnostic> diagnostics)
        : base(FormatMessage(diagnostics))
    {
        Diagnostics = [.. diagnostics];
    }

    private static string FormatMessage(IEnumerable<Diagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine, diagnostics.Select(d => d.Message));
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
