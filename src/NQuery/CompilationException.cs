using System.Collections.Immutable;

namespace NQuery
{
    public sealed class CompilationException : Exception
    {
        public CompilationException(IReadOnlyCollection<Diagnostic> diagnostics)
            : base(FormatMessage(diagnostics))
        {
            Diagnostics = diagnostics.ToImmutableArray();
        }

        private static string FormatMessage(IEnumerable<Diagnostic> diagnostics)
        {
            return string.Join(Environment.NewLine, diagnostics.Select(d => d.Message));
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}