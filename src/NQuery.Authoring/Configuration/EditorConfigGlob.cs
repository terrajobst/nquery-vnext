using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace NQuery.Authoring.Configuration;

// EditorConfig's section patterns, translated to a regular expression.
//
// A pattern containing a path separator is anchored at the directory of the .editorconfig that
// declared it; one without matches a name at any depth below it. Matching is case-insensitive: the
// spec doesn't say either way, implementations disagree, and a .NQL file that missed [*.nql] would
// only ever be read as a bug.
//
// Supported: * ** ? [seq] [!seq] {a,b} and backslash escapes. Numeric ranges ({1..9}) are not --
// braces without a comma stay literal, so such a pattern matches nothing rather than the wrong
// thing.
internal static class EditorConfigGlob
{
    // Windows compares paths case-insensitively; everywhere else two names differing in case are
    // two files. This is about locating the file below the config, not about matching the pattern,
    // which is case-insensitive everywhere.
    private static readonly StringComparison PathComparison =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    public static bool IsMatch(string pattern, string directory, string filePath)
    {
        var anchored = ContainsPathSeparator(pattern);

        if (!TryGetRelativePath(directory, filePath, out var relativePath))
        {
            // The file isn't below the config's directory, so nothing anchored there can reach it.
            // A pattern that only names a file still can.
            if (anchored)
                return false;

            relativePath = Path.GetFileName(filePath);
        }

        // Regex caches what the static overloads compile, which is what makes translating on every
        // call rather than holding on to the pattern affordable.
        return Regex.IsMatch(relativePath, Translate(pattern, anchored), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    // A / inside a character class is a literal, so it doesn't anchor the pattern.
    private static bool ContainsPathSeparator(string pattern)
    {
        var inClass = false;

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            if (c == '\\' && i + 1 < pattern.Length)
                i++;
            else if (c == '[')
                inClass = true;
            else if (c == ']')
                inClass = false;
            else if (c == '/' && !inClass)
                return true;
        }

        return false;
    }

    private static bool TryGetRelativePath(string directory, string filePath, out string relativePath)
    {
        var prefix = Normalize(directory).TrimEnd('/');
        var path = Normalize(filePath);

        if (prefix.Length > 0 &&
            path.Length > prefix.Length &&
            path[prefix.Length] == '/' &&
            path.StartsWith(prefix, PathComparison))
        {
            relativePath = path.Substring(prefix.Length + 1);
            return true;
        }

        relativePath = string.Empty;
        return false;
    }

    // Only where the platform actually separates paths that way: elsewhere a backslash is an
    // ordinary character in a file name and rewriting it would invent a directory.
    private static string Normalize(string path)
    {
        return Path.DirectorySeparatorChar == '\\' ? path.Replace('\\', '/') : path;
    }

    private static string Translate(string pattern, bool anchored)
    {
        var builder = new StringBuilder();

        // A pattern without a separator applies at any depth below the config, which is the same
        // as letting any directory prefix precede it.
        builder.Append(anchored ? @"^" : @"^(?:.*/)?");

        // A leading slash only anchors; it is not part of the relative path.
        var start = pattern.Length > 0 && pattern[0] == '/' ? 1 : 0;

        AppendPattern(builder, pattern, start, pattern.Length);

        builder.Append('$');
        return builder.ToString();
    }

    private static void AppendPattern(StringBuilder builder, string pattern, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            var c = pattern[i];

            switch (c)
            {
                case '\\' when i + 1 < end:
                    AppendLiteral(builder, pattern[++i]);
                    break;
                case '*':
                    i = AppendStar(builder, pattern, i, end);
                    break;
                case '?':
                    builder.Append(@"[^/]");
                    break;
                case '[':
                    i = AppendCharacterClass(builder, pattern, i, end);
                    break;
                case '{':
                    i = AppendAlternation(builder, pattern, i, end);
                    break;
                default:
                    AppendLiteral(builder, c);
                    break;
            }
        }
    }

    // ** crosses directories, * doesn't. A **/ swallows its own separator, so [**/*.nql] covers a
    // file sitting next to the config as well as one further down -- which is what anybody writing
    // that pattern means by it.
    private static int AppendStar(StringBuilder builder, string pattern, int i, int end)
    {
        if (i + 1 < end && pattern[i + 1] == '*')
        {
            if (i + 2 < end && pattern[i + 2] == '/')
            {
                builder.Append(@"(?:.*/)?");
                return i + 2;
            }

            builder.Append(@".*");
            return i + 1;
        }

        builder.Append(@"[^/]*");
        return i;
    }

    private static int AppendCharacterClass(StringBuilder builder, string pattern, int i, int end)
    {
        var negated = i + 1 < end && pattern[i + 1] == '!';
        var contentStart = negated ? i + 2 : i + 1;
        var close = IndexOfClosingBracket(pattern, contentStart, end);

        // An unterminated or empty class is just a bracket. Every glob syntax has to say this
        // somewhere, and an empty one has no regex spelling at all.
        if (close < 0 || close == contentStart)
        {
            AppendLiteral(builder, '[');
            return i;
        }

        builder.Append('[');

        if (negated)
            builder.Append('^');

        for (var j = contentStart; j < close; j++)
        {
            var c = pattern[j];

            if (c == '\\' && j + 1 < close)
                c = pattern[++j];

            // A dash goes through as written: a range means the same thing in both syntaxes.
            if (c is '\\' or ']' or '^' or '[')
                builder.Append('\\');

            builder.Append(c);
        }

        builder.Append(']');
        return close;
    }

    private static int IndexOfClosingBracket(string pattern, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            if (pattern[i] == '\\' && i + 1 < end)
                i++;
            else if (pattern[i] == ']')
                return i;
        }

        return -1;
    }

    private static int AppendAlternation(StringBuilder builder, string pattern, int i, int end)
    {
        var alternatives = GetAlternatives(pattern, i, end, out var close);

        // Braces without a top-level comma aren't an alternation, which is also how {1..9} ends up
        // literal rather than silently matching something else.
        if (alternatives is null)
        {
            AppendLiteral(builder, '{');
            return i;
        }

        builder.Append(@"(?:");

        for (var a = 0; a < alternatives.Count; a++)
        {
            if (a > 0)
                builder.Append('|');

            // Recursive, so an alternative is a pattern in its own right: {*.nql,src/*.nqe} works,
            // and so does nesting.
            AppendPattern(builder, pattern, alternatives[a].Start, alternatives[a].End);
        }

        builder.Append(')');
        return close;
    }

    private static List<(int Start, int End)>? GetAlternatives(string pattern, int i, int end, out int close)
    {
        var alternatives = new List<(int Start, int End)>();
        var start = i + 1;
        var depth = 0;
        var separated = false;

        close = -1;

        for (var j = start; j < end; j++)
        {
            var c = pattern[j];

            if (c == '\\' && j + 1 < end)
            {
                j++;
            }
            else if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                if (depth > 0)
                {
                    depth--;
                    continue;
                }

                alternatives.Add((start, j));
                close = j;
                return separated ? alternatives : null;
            }
            else if (c == ',' && depth == 0)
            {
                alternatives.Add((start, j));
                start = j + 1;
                separated = true;
            }
        }

        return null;
    }

    private static void AppendLiteral(StringBuilder builder, char c)
    {
        builder.Append(Regex.Escape(c.ToString()));
    }
}
