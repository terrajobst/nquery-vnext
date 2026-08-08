using NQuery.Authoring.LanguageServer.Protocol;

namespace NQuery.Authoring.LanguageServer.Mapping;

internal static class GlyphMapping
{
    extension(Glyph glyph)
    {
        public CompletionItemKind ToCompletionItemKind()
        {
            return glyph switch
            {
                Glyph.AmbiguousName => CompletionItemKind.Text,
                Glyph.Keyword => CompletionItemKind.Keyword,
                Glyph.Variable => CompletionItemKind.Variable,
                Glyph.Relation => CompletionItemKind.Reference,
                Glyph.Table => CompletionItemKind.Class,

                // A table instance is an alias bound in the query's scope, so it reads more like
                // a local than a type -- it deliberately shares an icon with Glyph.Variable.
                Glyph.TableInstance => CompletionItemKind.Variable,

                Glyph.Aggregate => CompletionItemKind.Function,
                Glyph.Column => CompletionItemKind.Field,
                Glyph.Function => CompletionItemKind.Function,
                Glyph.Method => CompletionItemKind.Method,
                Glyph.Property => CompletionItemKind.Property,
                Glyph.Type => CompletionItemKind.TypeParameter,
                _ => CompletionItemKind.Text
            };
        }
    }
}
