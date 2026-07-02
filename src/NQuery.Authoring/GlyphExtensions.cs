using NQuery.CodeAnalysis;

namespace NQuery.Authoring;

public static class GlyphExtensions
{
    extension(Symbol symbol)
    {
        public Glyph GetGlyph()
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return Glyph.Column;
                case SymbolKind.Table:
                    return Glyph.Table;
                case SymbolKind.TableInstance:
                    return Glyph.TableInstance;
                case SymbolKind.ColumnInstance:
                    return Glyph.Column;
                case SymbolKind.Variable:
                    return Glyph.Variable;
                case SymbolKind.Function:
                    return Glyph.Function;
                case SymbolKind.Aggregate:
                    return Glyph.Aggregate;
                case SymbolKind.Property:
                    return Glyph.Property;
                case SymbolKind.Method:
                    return Glyph.Method;
                default:
                    throw ExceptionBuilder.UnexpectedValue(symbol.Kind);
            }
        }
    }
}
