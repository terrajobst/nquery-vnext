using System;

namespace NQuery.Language
{
    public sealed class Diagnostic
    {
        private readonly TextSpan _textSpan;
        private readonly DiagnosticId _diagnosticId;
        private readonly string _message;

        public Diagnostic(TextSpan textSpan, DiagnosticId diagnosticId, string message)
        {
            _textSpan = textSpan;
            _diagnosticId = diagnosticId;
            _message = message;
        }

        public TextSpan Span
        {
            get { return _textSpan; }
        }

        public DiagnosticId DiagnosticId
        {
            get { return _diagnosticId; }
        }

        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return Message;
        }
    }
}