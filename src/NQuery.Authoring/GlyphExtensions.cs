using System;

namespace NQuery.Authoring
{
    public static class GlyphExtensions
    {
        public static Glyph GetGlyph(this Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return Glyph.Column;
                case SymbolKind.SchemaTable:
                case SymbolKind.DerivedTable:
                case SymbolKind.CommonTableExpression:
                    return Glyph.Table;
                case SymbolKind.TableInstance:
                    return Glyph.TableInstance;
                case SymbolKind.TableColumnInstance:
                case SymbolKind.QueryColumnInstance:
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
                    throw new NotImplementedException($"Unknown symbol kind: {symbol.Kind}");
            }
        }
    }
}