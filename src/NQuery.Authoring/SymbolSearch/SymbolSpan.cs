﻿using NQuery.Text;

namespace NQuery.Authoring.SymbolSearch
{
    public struct SymbolSpan : IEquatable<SymbolSpan>
    {
        public SymbolSpan(SymbolSpanKind kind, Symbol symbol, TextSpan span)
        {
            ArgumentNullException.ThrowIfNull(symbol);

            Kind = kind;
            Symbol = symbol;
            Span = span;
        }

        public SymbolSpanKind Kind { get; }

        public Symbol Symbol { get; }

        public TextSpan Span { get; }

        public bool Equals(SymbolSpan other)
        {
            return Kind == other.Kind &&
                   Symbol == other.Symbol &&
                   Span == other.Span;
        }

        public override bool Equals(object obj)
        {
            return obj is SymbolSpan other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Symbol, Span);
        }

        public static SymbolSpan CreateReference(Symbol symbol, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Reference, symbol, span);
        }

        public static SymbolSpan CreateDefinition(Symbol symbol, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Definition, symbol, span);
        }
    }
}