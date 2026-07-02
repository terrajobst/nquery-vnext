using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery;

public static class StringExtensions
{
    extension(string text)
    {
        public string Substring(TextSpan span)
        {
            return text.Substring(span.Start, span.Length);
        }

        public string ParseSpans(out ImmutableArray<TextSpan> spans)
        {
            var annotatedText = AnnotatedText.Parse(text);
            spans = annotatedText.Spans;
            return annotatedText.Text;
        }

        public string ParseSinglePosition(out int position)
        {
            var annotatedText = AnnotatedText.Parse(text);
            if (annotatedText.Spans.Length != 1 || annotatedText.Spans[0].Length != 0)
                throw new ArgumentException(@"The position must be marked with a single pipe, such as 'SELECT e.Empl|oyeeId'", nameof(text));

            position = annotatedText.Spans.Single().Start;
            return annotatedText.Text;
        }

        public string ParseSingleSpan(out TextSpan span)
        {
            var result = text.ParseSpans(out var spans);

            if (spans.Length != 1)
                throw new ArgumentException(@"The span must be marked with braces, such as 'SELECT {e.EmployeeId}'", nameof(text));

            span = spans[0];
            return result;
        }
    }
}
