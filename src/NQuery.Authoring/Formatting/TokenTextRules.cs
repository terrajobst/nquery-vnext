using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Formatting;

// The two passes that rewrite a token's own text rather than the space around it. Both are pure
// per-token maps, which is what lets a host ask for casing without layout, or neither.
internal static class TokenTextRules
{
    public static string GetText(SyntaxToken token, FormattingOptions options)
    {
        var text = token.Text;

        if (token.IsMissing || text.Length == 0)
            return text;

        // A contextual keyword used as a keyword has had its kind rewritten by the parser, and one
        // used as a name hasn't, so the tree already answers "is this a keyword here".
        if (token.Kind.IsKeyword())
        {
            return options.Keywords switch
            {
                Casing.Upper => text.ToUpperInvariant(),
                Casing.Lower => text.ToLowerInvariant(),
                _ => text
            };
        }

        if (token.Kind == SyntaxKind.IdentifierToken && options.Identifiers == IdentifierQuoting.WhenRequired)
            return Unquote(token, text);

        return text;
    }

    private static string Unquote(SyntaxToken token, string text)
    {
        if (!token.IsQuotedIdentifier() && !token.IsParenthesizedIdentifier())
            return text;

        var name = token.ValueText;

        if (!SyntaxFacts.IsValidIdentifier(name))
            return text;

        // IsValidIdentifier only rules out the reserved words. Unbracketing a contextual keyword
        // compiles, but it can change what the parser does with it -- which is a rename, not a
        // formatting change.
        if (SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.IdentifierToken)
            return text;

        return name;
    }
}
