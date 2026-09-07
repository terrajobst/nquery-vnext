using System.Collections.Immutable;

namespace NQuery.Authoring.Configuration;

// One .editorconfig as it was written: the sections in file order, plus the preamble's root flag.
// Nothing here is matched against a file yet, which is what keeps a config readable once and
// applicable to many files.
//
// Parsing never fails. A line that is neither a comment, a section header nor a pair is dropped:
// the file belongs to the repository rather than to us, it is routinely written for other tools,
// and refusing to format because of a line meant for someone else would be the wrong trade.
internal sealed class EditorConfigFile
{
    private static readonly string[] LineSeparators = ["\r\n", "\n", "\r"];

    private EditorConfigFile(bool isRoot, ImmutableArray<EditorConfigSection> sections)
    {
        IsRoot = isRoot;
        Sections = sections;
    }

    public bool IsRoot { get; }

    public ImmutableArray<EditorConfigSection> Sections { get; }

    public static EditorConfigFile Parse(string text)
    {
        var isRoot = false;
        var sections = ImmutableArray.CreateBuilder<EditorConfigSection>();
        var properties = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
        var pattern = (string?)null;

        foreach (var line in text.Split(LineSeparators, StringSplitOptions.None))
        {
            var trimmed = line.Trim();

            // A ; or # starts a comment only at the beginning of a line. Anywhere else it is part
            // of the text, so values are never truncated at one.
            if (trimmed.Length == 0 || trimmed[0] == ';' || trimmed[0] == '#')
                continue;

            if (trimmed[0] == '[' && trimmed[trimmed.Length - 1] == ']')
            {
                if (pattern is not null)
                    sections.Add(new EditorConfigSection(pattern, properties.ToImmutable()));

                pattern = trimmed.Substring(1, trimmed.Length - 2);
                properties.Clear();
                continue;
            }

            var separator = trimmed.IndexOf('=');
            if (separator < 0)
                continue;

            var key = trimmed.Substring(0, separator).Trim().ToLowerInvariant();
            var value = trimmed.Substring(separator + 1).Trim();

            if (key.Length == 0)
                continue;

            // Outside a section, root is the only key with any effect -- and the only one allowed
            // to be there at all.
            if (pattern is null)
            {
                if (key == @"root")
                    isRoot = string.Equals(value, @"true", StringComparison.OrdinalIgnoreCase);

                continue;
            }

            properties.Add(new KeyValuePair<string, string>(key, value));
        }

        if (pattern is not null)
            sections.Add(new EditorConfigSection(pattern, properties.ToImmutable()));

        return new EditorConfigFile(isRoot, sections.ToImmutable());
    }
}

// Kept as a list rather than a dictionary because a key repeated inside one section is legal and
// the later one wins, which falls out of applying them in order.
internal sealed class EditorConfigSection
{
    public EditorConfigSection(string pattern, ImmutableArray<KeyValuePair<string, string>> properties)
    {
        Pattern = pattern;
        Properties = properties;
    }

    public string Pattern { get; }

    public ImmutableArray<KeyValuePair<string, string>> Properties { get; }
}
