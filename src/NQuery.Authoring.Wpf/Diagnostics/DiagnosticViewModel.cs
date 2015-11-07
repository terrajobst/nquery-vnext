using System;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticViewModel
    {
        public DiagnosticViewModel(Diagnostic diagnostic, SourceText sourceText)
        {
            var textLocation = sourceText.GetTextLocation(diagnostic.Span.Start);
            Diagnostic = diagnostic;
            Description = diagnostic.Message;
            Column = textLocation.Column + 1;
            Line = textLocation.Line + 1;
        }

        public Diagnostic Diagnostic { get; }

        public string Description { get; }

        public int Line { get; }

        public int Column { get; }
    }
}