namespace NQuery.Authoring.Tests;

// A directory that exists for the length of one test, for the handful of features that read files
// rather than text: EditorConfig walks real directories, so there has to be something to walk.
internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Root = Path.Combine(Path.GetTempPath(), $"nquery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string CreateFile(string relativePath, string content)
    {
        var path = GetPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    // The file doesn't have to exist: a document that was never saved still has a name.
    public string GetPath(string relativePath)
    {
        return Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    public void Dispose()
    {
        Directory.Delete(Root, recursive: true);
    }
}
