using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Wpf;

internal sealed class DiagnosticsViewModel
{
    public DiagnosticsViewModel(IEnumerable<Diagnostic> diagnostics, SourceText sourceText)
    {
        Diagnostics = [.. (from d in diagnostics
                       orderby d.Span.Start, d.Span.End
                       select new DiagnosticViewModel(d, sourceText))];
    }

    public ImmutableArray<DiagnosticViewModel> Diagnostics { get; }
}
