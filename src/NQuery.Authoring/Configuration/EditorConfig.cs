using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NQuery.Authoring.Configuration;

// The EditorConfig properties that apply to a single file, resolved: sections matched in order,
// nearer files having overwritten farther ones, "unset" values taken back out again.
//
// Deliberately agnostic about which keys exist. EditorConfig is an open vocabulary -- the standard
// properties, whatever a repository already carries for other tools, and any nquery_* keys we add
// later -- so access is typed by key rather than by named member, and knowing what a key means is
// the caller's job. That is also where the awkward values belong: indent_size = tab and
// max_line_length = off aren't integers, so TryGetInt32 simply says no and the caller decides what
// to fall back to.
//
// This is I/O with no caching. Every resolution re-reads every .editorconfig on the way up, which
// is affordable because the files are small and the thing asking is a user-initiated format.
public sealed class EditorConfig
{
    public const string FileName = @".editorconfig";

    private EditorConfig(FrozenDictionary<string, string> properties)
    {
        Properties = properties;
    }

    public static EditorConfig Empty { get; } = new(FrozenDictionary<string, string>.Empty);

    // Keys are lowercased on the way in, as the spec requires, and looked up case-insensitively;
    // values keep the case they were written with, because plenty of them are free-form text.
    public FrozenDictionary<string, string> Properties { get; }

    // A single config, already read. configPath is where that text lives, which is what any section
    // containing a path separator is anchored to -- a pattern can't be matched without knowing the
    // directory it was written in.
    public static EditorConfig Parse(string text, string configPath, string filePath)
    {
        ThrowIfNull(text);
        ThrowIfNullOrEmpty(configPath);
        ThrowIfNullOrEmpty(filePath);

        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Apply(properties, EditorConfigFile.Parse(text), configPath, filePath);
        return Create(properties);
    }

    // One config, no walk and no root handling: this answers what that file alone says about
    // filePath, which is also the only sensible thing to ask about a config the caller picked.
    public static EditorConfig Load(string configPath, string filePath)
    {
        ThrowIfNullOrEmpty(configPath);
        ThrowIfNullOrEmpty(filePath);

        return Parse(File.ReadAllText(configPath), configPath, filePath);
    }

    // Walks up from the file's own directory and stops after the first config declaring root = true.
    // The configs are applied farthest first so that nearer ones overwrite them.
    //
    // filePath doesn't have to exist. An editor asks about a document, and a document that was
    // never saved still has a name -- only its directory is read here.
    public static EditorConfig LoadForFile(string filePath)
    {
        ThrowIfNullOrEmpty(filePath);

        var fullPath = Path.GetFullPath(filePath);
        var configs = new List<(EditorConfigFile File, string Path)>();
        var directory = Path.GetDirectoryName(fullPath);

        while (directory is { Length: > 0 })
        {
            var configPath = Path.Combine(directory, FileName);

            if (File.Exists(configPath))
            {
                var file = EditorConfigFile.Parse(File.ReadAllText(configPath));
                configs.Add((file, configPath));

                if (file.IsRoot)
                    break;
            }

            directory = Path.GetDirectoryName(directory);
        }

        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = configs.Count - 1; i >= 0; i--)
            Apply(properties, configs[i].File, configs[i].Path, fullPath);

        return Create(properties);
    }

    public bool TryGetString(string key, [NotNullWhen(true)] out string? value)
    {
        ThrowIfNullOrEmpty(key);

        return Properties.TryGetValue(key, out value);
    }

    public bool TryGetInt32(string key, out int value)
    {
        value = 0;

        return TryGetString(key, out var text) &&
               int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    // true and false and nothing else. EditorConfig has no second spelling for a boolean, so
    // accepting one would only turn a typo into a deliberate-looking answer.
    public bool TryGetBoolean(string key, out bool value)
    {
        value = false;

        if (!TryGetString(key, out var text))
            return false;

        if (string.Equals(text, @"true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        return string.Equals(text, @"false", StringComparison.OrdinalIgnoreCase);
    }

    private static void Apply(Dictionary<string, string> properties, EditorConfigFile file, string configPath, string filePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(configPath)) ?? string.Empty;
        var fullPath = Path.GetFullPath(filePath);

        foreach (var section in file.Sections)
        {
            if (!EditorConfigGlob.IsMatch(section.Pattern, directory, fullPath))
                continue;

            foreach (var property in section.Properties)
            {
                // "unset" doesn't set the key to the text "unset", it takes back whatever an
                // earlier section or a farther config had to say about it.
                if (string.Equals(property.Value, @"unset", StringComparison.OrdinalIgnoreCase))
                    properties.Remove(property.Key);
                else
                    properties[property.Key] = property.Value;
            }
        }
    }

    private static EditorConfig Create(Dictionary<string, string> properties)
    {
        return properties.Count == 0
                ? Empty
                : new EditorConfig(properties.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
    }
}
