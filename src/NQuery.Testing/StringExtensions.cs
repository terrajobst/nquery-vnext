using System.Collections.Immutable;
using System.Text;

using NQuery.Text;

namespace NQuery
{
    public static class StringExtensions
    {
        public static string NormalizeCode(this string text)
        {
            return text.Unindent().Trim();
        }

        public static string Unindent(this string text)
        {
            var minIndent = int.MaxValue;

            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) is not null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var indent = line.Length - line.TrimStart().Length;
                    minIndent = Math.Min(minIndent, indent);
                }
            }

            var sb = new StringBuilder();
            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) is not null)
                {
                    var unindentedLine = line.Length < minIndent
                        ? line
                        : line.Substring(minIndent);
                    sb.AppendLine(unindentedLine);
                }
            }

            return sb.ToString();
        }

        public static string Substring(this string text, TextSpan span)
        {
            return text.Substring(span.Start, span.Length);
        }

        public static string ParseSpans(this string text, out ImmutableArray<TextSpan> spans)
        {
            var annotatedText = AnnotatedText.Parse(text);
            spans = annotatedText.Spans;
            return annotatedText.Text;
        }

        public static string ParseSinglePosition(this string text, out int position)
        {
            var annotatedText = AnnotatedText.Parse(text);
            if (annotatedText.Spans.Length != 1 || annotatedText.Spans[0].Length != 0)
                throw new ArgumentException(@"The position must be marked with a single pipe, such as 'SELECT e.Empl|oyeeId'", nameof(text));

            position = annotatedText.Spans.Single().Start;
            return annotatedText.Text;
        }

        public static string ParseSingleSpan(this string text, out TextSpan span)
        {
            var result = text.ParseSpans(out var spans);

            if (spans.Length != 1)
                throw new ArgumentException(@"The span must be marked with braces, such as 'SELECT {e.EmployeeId}'", nameof(text));

            span = spans[0];
            return result;
        }
    }
}