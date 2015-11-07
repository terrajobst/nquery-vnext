using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class SymbolMarkup : IEquatable<SymbolMarkup>
    {
        public SymbolMarkup(IEnumerable<SymbolMarkupToken> tokens)
        {
            Tokens = tokens.ToImmutableArray();
        }

        public ImmutableArray<SymbolMarkupToken> Tokens { get; }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolMarkup;
            return other != null && Equals(other);
        }

        public bool Equals(SymbolMarkup other)
        {
            if (other.Tokens.Length != Tokens.Length)
                return false;

            for (var i = 0; i < Tokens.Length; i++)
            {
                if (!Tokens[i].Equals(other.Tokens[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Tokens.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(Tokens.Select(n => n.Text));
        }

        public static SymbolMarkup ForSymbol(Symbol symbol)
        {
            var nodes = new List<SymbolMarkupToken>();
            nodes.AppendSymbol(symbol);
            return new SymbolMarkup(nodes);
        }

        public static SymbolMarkup ForCastSymbol()
        {
            var nodes = new List<SymbolMarkupToken>();
            nodes.AppendCastSymbol();
            return new SymbolMarkup(nodes);
        }

        public static SymbolMarkup ForCoalesceSymbol()
        {
            var nodes = new List<SymbolMarkupToken>();
            nodes.AppendCoalesceSymbol();
            return new SymbolMarkup(nodes);
        }

        public static SymbolMarkup ForNullIfSymbol()
        {
            var nodes = new List<SymbolMarkupToken>();
            nodes.AppendNullIfSymbol();
            return new SymbolMarkup(nodes);
        }
    }
}