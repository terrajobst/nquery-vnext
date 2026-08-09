using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.LanguageServer.Mapping;

internal static class SymbolMarkupMapping
{
    // Hover carries markdown, not classified runs, so ToString() is as much as the protocol can
    // take: it concatenates the token text and drops the SymbolMarkupKind of each one. The client
    // gets the colors back by re-lexing the fenced text.
    //
    // The fence names a language of its own rather than "nquery". A declaration line is not a
    // query -- it is keyword-led ("COLUMN Customers.CompanyName AS STRING"), and that keyword is
    // exactly what lets a grammar tell a table name from a column name, which is undecidable in a
    // real query where both are bare identifiers. See vscode/syntaxes/nquery-quickinfo.tmLanguage.json.
    private const string QuickInfoLanguage = @"nquery-quickinfo";

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

        return $"```{QuickInfoLanguage}{Environment.NewLine}{text}{Environment.NewLine}```";
    }
}
