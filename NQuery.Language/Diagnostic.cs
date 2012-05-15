using System;

namespace NQuery.Language
{
    public sealed class Diagnostic
    {
        private readonly SyntaxNodeOrToken _nodeOrToken;
        private readonly DiagnosticId _diagnosticId;
        private readonly string _message;

        public Diagnostic(SyntaxNodeOrToken nodeOrToken, DiagnosticId diagnosticId, string message)
        {
            _nodeOrToken = nodeOrToken;
            _diagnosticId = diagnosticId;
            _message = message;
        }

        public SyntaxNodeOrToken NodeOrToken
        {
            get { return _nodeOrToken; }
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