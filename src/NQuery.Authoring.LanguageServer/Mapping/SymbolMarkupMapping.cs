using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.LanguageServer.Mapping;

internal static class SymbolMarkupMapping
{
    // The markup's own ToString() concatenates the token text, which is exactly the declaration
    // line we want. Wrapping it in an nquery fence lets the client colorize it with the same
    // grammar the editor uses for the document itself, so hover matches the buffer.
    extension(SymbolMarkup markup)
    {
        public MarkupContent ToMarkupContent()
        {
            ThrowIfNull(markup);

            return MarkupContent.Markdown(FencedCode(markup.ToString()));
        }
    }

    public static string FencedCode(string text)
    {
        ThrowIfNull(text);

        return $"```nquery{Environment.NewLine}{text}{Environment.NewLine}```";
    }
}
