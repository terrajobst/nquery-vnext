using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticsViewModel
    {
        private readonly ImmutableArray<DiagnosticViewModel> _diagnostics;

        public DiagnosticsViewModel(IEnumerable<Diagnostic> diagnostics, TextBuffer textBuffer)
        {
            _diagnostics = (from d in diagnostics
                            orderby d.Span.Start, d.Span.End
                            select new DiagnosticViewModel(d, textBuffer)).ToImmutableArray();
        }

        public ImmutableArray<DiagnosticViewModel> Diagnostics
        {
            get { return _diagnostics; }
        }
    }
}