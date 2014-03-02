using System;

namespace NQuery.Authoring.UnitTests
{
    internal static class StringExtensions
    {
        public static string Substring(this string text, TextSpan span)
        {
            return text.Substring(span.Start, span.Length);
        }
    }
}