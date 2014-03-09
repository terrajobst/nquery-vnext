using System;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class DiagnosticViewModel
    {
        private readonly Diagnostic _diagnostic;
        private readonly string _description;
        private readonly int _line;
        private readonly int _column;

        public DiagnosticViewModel(Diagnostic diagnostic, TextBuffer textBuffer)
        {
            var textLocation = textBuffer.GetTextLocation(diagnostic.Span.Start);
            _diagnostic = diagnostic;
            _description = diagnostic.Message;
            _column = textLocation.Column + 1;
            _line = textLocation.Line + 1;
        }

        public Diagnostic Diagnostic
        {
            get { return _diagnostic; }
        }

        public string Description
        {
            get { return _description; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int Column
        {
            get { return _column; }
        }
    }
}