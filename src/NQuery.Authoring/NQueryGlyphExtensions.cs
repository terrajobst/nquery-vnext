using System;

using NQuery.Symbols;

namespace NQuery.Authoring
{
    public static class NQueryGlyphExtensions
    {
        public static NQueryGlyph GetGlyph(this Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return NQueryGlyph.Column;
                case SymbolKind.SchemaTable:
                case SymbolKind.DerivedTable:
                case SymbolKind.CommonTableExpression:
                    return NQueryGlyph.Table;
                case SymbolKind.TableInstance:
                    return NQueryGlyph.TableInstance;
                case SymbolKind.TableColumnInstance:
                case SymbolKind.QueryColumnInstance:
                    return NQueryGlyph.Column;
                case SymbolKind.Variable:
                    return NQueryGlyph.Variable;
                case SymbolKind.Function:
                    return NQueryGlyph.Function;
                case SymbolKind.Aggregate:
                    return NQueryGlyph.Aggregate;
                case SymbolKind.Property:
                    return NQueryGlyph.Property;
                case SymbolKind.Method:
                    return NQueryGlyph.Method;
                default:
                    throw new NotImplementedException(string.Format("Unknown symbol kind: {0}", symbol.Kind));
            }
        }
    }
}