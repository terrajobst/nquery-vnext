using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using NQuery.Text;

namespace NQuery.Authoring.Tests
{
    internal static class StringExtensions
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
                while ((line = stringReader.ReadLine()) != null)
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
                while ((line = stringReader.ReadLine()) != null)
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
                throw new ArgumentException("The position must be marked with a single pipe, such as 'SELECT e.Empl|oyeeId'");

            position = annotatedText.Spans.Single().Start;
            return annotatedText.Text;
        }

        public static string ParseSingleSpan(this string text, out TextSpan span)
        {
            ImmutableArray<TextSpan> spans;
            var result = text.ParseSpans(out spans);

            if (spans.Length != 1)
                throw new ArgumentException("The span must be marked with braces, such as 'SELECT {e.EmployeeId}'");

            span = spans[0];
            return result;
        }
    }
}