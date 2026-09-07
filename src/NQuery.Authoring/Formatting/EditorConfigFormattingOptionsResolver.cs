using NQuery.Authoring.Configuration;

namespace NQuery.Authoring.Formatting;

// Lets the .editorconfig files above a document override the options it would otherwise be
// formatted with.
//
// Not part of AddDefaultServices, deliberately: this is the one service in the authoring layer that
// touches the file system, so a host asks for it rather than getting it by accident. Registering it
// means replacing the default rather than adding to it:
//
//     AuthoringServices.Create(builder =>
//     {
//         builder.AddDefaultServices();
//         builder.RemoveServices<FormattingOptionsResolver>();
//         builder.AddService<FormattingOptionsResolver, EditorConfigFormattingOptionsResolver>();
//     });
//
// Every call re-reads every config on the way up. Formatting is user-initiated and the files are
// small, and a cache would need invalidation the authoring layer has no way to hear about.
public sealed class EditorConfigFormattingOptionsResolver : FormattingOptionsResolver
{
    public override FormattingOptions GetOptions(Document document, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        // A buffer that was never saved has no directory to walk up from, and inventing one would
        // apply somebody else's settings to it.
        if (document.FilePath is null)
            return options;

        cancellationToken.ThrowIfCancellationRequested();

        return options.WithEditorConfig(EditorConfig.LoadForFile(document.FilePath));
    }
}
