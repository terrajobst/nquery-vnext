using System;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticViewModel
    {
        private DiagnosticViewModel(string description, TextSpan span, SourceText sourceText)
        {
            Description = description;
            Span = span;
            var textLocation = sourceText.GetTextLocation(span.Start);
            Column = textLocation.Column + 1;
            Line = textLocation.Line + 1;
        }

        public static DiagnosticViewModel ForDiagnostic(Diagnostic diagnostic, SourceText sourceText)
        {
            return new DiagnosticViewModel(diagnostic.Message, diagnostic.Span, sourceText);
        }
        public static DiagnosticViewModel ForIssue(CodeIssue codeIssue, SourceText sourceText)
        {
            return new DiagnosticViewModel(codeIssue.Description, codeIssue.Span, sourceText);
        }

        public string Description { get; }

        public TextSpan Span { get; }

        public int Line { get; }

        public int Column { get; }
    }
}