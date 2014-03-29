using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace NQuery
{
    public sealed class CompilationException : Exception
    {
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        public CompilationException(IReadOnlyCollection<Diagnostic> diagnostics)
            : base(FormatMessage(diagnostics))
        {
            _diagnostics = diagnostics.ToImmutableArray();
        }

        private static string FormatMessage(IEnumerable<Diagnostic> diagnostics)
        {
            var sb = new StringBuilder();

            foreach (var diagnostic in diagnostics)
                sb.AppendLine(diagnostic.Message);

            return sb.ToString();
        }

        public ImmutableArray<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }
    }
}