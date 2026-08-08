using System.Text.Json;

namespace NQuery.Authoring.LanguageServer.Hosting;

// One server process serves one .nqproj project, so the project is resolved once at initialize
// and handed to the catalog provider factory. Settings is the opaque blob from the project
// file's "settings" member -- the VS Code extension never looks inside it.
public sealed class ProjectContext
{
    public ProjectContext(Uri? projectFile, string projectName, JsonElement settings, ILanguageServerHost host)
    {
        ThrowIfNull(projectName);
        ThrowIfNull(host);

        ProjectFile = projectFile;
        ProjectName = projectName;
        Settings = settings;
        Host = host;
    }

    public Uri? ProjectFile { get; }

    public string ProjectName { get; }

    public JsonElement Settings { get; }

    public ILanguageServerHost Host { get; }

    public T? GetSetting<T>(string name, T? defaultValue = default)
    {
        ThrowIfNull(name);

        if (Settings.ValueKind != JsonValueKind.Object)
            return defaultValue;

        if (!Settings.TryGetProperty(name, out var value))
            return defaultValue;

        try
        {
            return value.Deserialize<T>();
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }
}
