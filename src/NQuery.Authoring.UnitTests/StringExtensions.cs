using System;

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
    }
}