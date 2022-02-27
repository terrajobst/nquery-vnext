using System.Globalization;

using NQuery.Text;

namespace NQuery
{
    public sealed class Diagnostic
    {
        public Diagnostic(TextSpan textSpan, DiagnosticId diagnosticId, string message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            Span = textSpan;
            DiagnosticId = diagnosticId;
            Message = message;
        }

        public static Diagnostic Format(TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var message = diagnosticId.GetMessage();
            var formattedMessage = string.Format(CultureInfo.CurrentCulture, message, args);
            return new Diagnostic(textSpan, diagnosticId, formattedMessage);
        }

        public TextSpan Span { get; }

        public DiagnosticId DiagnosticId { get; }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}