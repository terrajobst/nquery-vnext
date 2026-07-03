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
        // Runs while evaluating the base constructor argument, i.e. before any field
        // assignment, so this is where the public constructor's argument is validated.
        ThrowIfNull(diagnostics);

        return string.Join(Environment.NewLine, diagnostics.Select(d => d.Message));
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
