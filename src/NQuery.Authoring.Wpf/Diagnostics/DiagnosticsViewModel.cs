using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(IEnumerable<Diagnostic> diagnostics, IEnumerable<CodeIssue> codeIssues, SourceText sourceText)
        {
            var diagnosticViewModels = diagnostics.Select(d => DiagnosticViewModel.ForDiagnostic(d, sourceText));
            var codeIssueViewModels = codeIssues.Select(i => DiagnosticViewModel.ForIssue(i, sourceText));
            Diagnostics = diagnosticViewModels.Concat(codeIssueViewModels)
                                              .OrderBy(d => d.Span.Start)
                                              .ThenBy(d => d.Span.End)
                                              .ToImmutableArray();
        }

        public ImmutableArray<DiagnosticViewModel> Diagnostics { get; }
    }
}