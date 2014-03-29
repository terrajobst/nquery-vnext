using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class SymbolMarkup : IEquatable<SymbolMarkup>
    {
        private readonly ImmutableArray<SymbolMarkupToken> _tokens;

        public SymbolMarkup(IEnumerable<SymbolMarkupToken> tokens)
        {
            _tokens = tokens.ToImmutableArray();
        }

        public ImmutableArray<SymbolMarkupToken> Tokens
        {
            get { return _tokens; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolMarkup;
            return other != null && Equals(other);
        }

        public bool Equals(SymbolMarkup other)
        {
            if (other.Tokens.Length != _tokens.Length)
                return false;

            for (var i = 0; i < _tokens.Length; i++)
            {
                if (!_tokens[i].Equals(other.Tokens[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _tokens.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(_tokens.Select(n => n.Text));
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