using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticsViewModel
    {
        private readonly ReadOnlyCollection<DiagnosticViewModel> _diagnostics;

        public DiagnosticsViewModel(IEnumerable<Diagnostic> diagnostics, TextBuffer textBuffer)
        {
            var viewModels = (from d in diagnostics
                              orderby d.Span.Start, d.Span.End
                              select new DiagnosticViewModel(d, textBuffer)).ToList();


            _diagnostics = new ReadOnlyCollection<DiagnosticViewModel>(viewModels);
        }

        public ReadOnlyCollection<DiagnosticViewModel> Diagnostics
        {
            get { return _diagnostics; }
        }
    }
}