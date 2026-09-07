namespace NQuery.Authoring.Formatting;

// Turns the options a caller already has into the options a document should actually be formatted
// with. The default is to change nothing, which is what keeps this layer free of I/O: a host that
// wants a config file consulted registers a resolver that reads one.
//
// The options coming in are the baseline rather than a suggestion, and a resolver only overrides
// what it can actually answer for. That is what makes the precedence chain fall out of the call
// sites -- the LSP server hands in its preset merged with the client's request, and whatever an
// .editorconfig says then wins over both -- rather than being a rule written down somewhere and
// kept true by hand.
public class FormattingOptionsResolver
{
    public virtual FormattingOptions GetOptions(Document document, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        return options;
    }
}
