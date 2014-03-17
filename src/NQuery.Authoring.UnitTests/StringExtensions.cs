using System;
using System.Collections.Generic;
using System.Text;

using NQuery.Text;

namespace NQuery.Authoring.UnitTests
{
    internal static class StringExtensions
    {
        public static string Substring(this string text, TextSpan span)
        {
            return text.Substring(span.Start, span.Length);
        }

        public static string NormalizeLineEnding(this string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        public static string ParseSpans(this string text, out TextSpan[] spans)
        {
            var resultSpans = new List<TextSpan>();
            var sb = new StringBuilder();
            var spanStart = (int?) null;
            foreach (var c in text)
            {
                switch (c)
                {
                    case '{':
                        if (spanStart != null)
                            throw new FormatException("Nested braces are illegal");
                        spanStart = sb.Length;
                        break;
                    case '}':
                        if (spanStart == null)
                            throw new FormatException("Missing open brace");

                        resultSpans.Add(TextSpan.FromBounds(spanStart.Value, sb.Length));
                        spanStart = null;
                        break;
                    case '|':
                        resultSpans.Add(new TextSpan(sb.Length, 0));
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            spans = resultSpans.ToArray();
            return sb.ToString();
        }

        public static string ParseSinglePosition(this string text, out int position)
        {
            TextSpan[] spans;
            var result = text.ParseSpans(out spans);

            if (spans.Length != 1 || spans[0].Length != 0)
                throw new ArgumentException("The position must be marked with a single pipe, such as 'SELECT e.Empl|oyeeId'");

            position = spans[0].Start;
            return result;
        }

        public static string ParseSingleSpan(this string text, out TextSpan span)
        {
            TextSpan[] spans;
            var result = text.ParseSpans(out spans);

            if (spans.Length != 1)
                throw new ArgumentException("The span must be marked with braces, such as 'SELECT {e.EmployeeId}'");

            span = spans[0];
            return result;
        }
    }
}