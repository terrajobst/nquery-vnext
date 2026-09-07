using System.Text;

namespace NQuery.Authoring.Formatting;

// The line breaks the formatter doesn't lay out: the ones inside a string literal, which is the
// only token whose text can span lines (a quoted or bracketed identifier is terminated by one), and
// the ones inside a multi line comment. Every other break is rendered from scratch out of NewLine,
// so leaving these alone is what would give a converted file mixed endings.
//
// Rewriting the breaks inside a string literal does change the value the query computes. That is
// the same thing that already happens whenever such a file is checked out on a platform with
// different endings, and a formatter that left one CRLF behind in an otherwise-LF file would be
// the more surprising of the two.
internal static class LineBreakRules
{
    private static readonly char[] LineBreakChars = ['\r', '\n'];

    public static string Normalize(string text, string newLine)
    {
        var start = text.IndexOfAny(LineBreakChars);

        // Nothing to convert, which is every token that isn't a multi-line literal and every
        // single line comment.
        if (start < 0)
            return text;

        var builder = new StringBuilder(text.Length);
        builder.Append(text, 0, start);

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '\r')
            {
                // A lone CR is a line break of its own, so only the pair is consumed together.
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;

                builder.Append(newLine);
            }
            else if (c == '\n')
            {
                builder.Append(newLine);
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
