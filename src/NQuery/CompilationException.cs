using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NQuery
{
    public sealed class CompilationException : Exception
    {
        private readonly IReadOnlyCollection<Diagnostic> _diagnostics;

        public CompilationException(IList<Diagnostic> diagnostics)
            : base(FormatMessage(diagnostics))
        {
            _diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }

        private static string FormatMessage(IEnumerable<Diagnostic> diagnostics)
        {
            var sb = new StringBuilder();

            foreach (var diagnostic in diagnostics)
                sb.AppendLine(diagnostic.Message);

            return sb.ToString();
        }

        public IReadOnlyCollection<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }
    }
}