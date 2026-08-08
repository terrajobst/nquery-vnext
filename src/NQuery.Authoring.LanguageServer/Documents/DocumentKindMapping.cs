namespace NQuery.Authoring.LanguageServer.Documents;

// .nql holds a query, .nqe holds a bare expression. Anything else -- including a .sql file the
// user mapped onto the nquery language via files.associations -- is treated as a query.
public static class DocumentKindMapping
{
    public const string QueryExtension = @".nql";
    public const string ExpressionExtension = @".nqe";

    public static DocumentKind FromUri(Uri uri)
    {
        ThrowIfNull(uri);

        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;
        return FromPath(path);
    }

    public static DocumentKind FromPath(string path)
    {
        ThrowIfNull(path);

        var extension = Path.GetExtension(path);
        return string.Equals(extension, ExpressionExtension, StringComparison.OrdinalIgnoreCase)
            ? DocumentKind.Expression
            : DocumentKind.Query;
    }
}
