using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class SymbolMarkup : IEquatable<SymbolMarkup>
    {
        private readonly ReadOnlyCollection<SymbolMarkupToken> _tokens;

        public SymbolMarkup(IList<SymbolMarkupToken> tokens)
        {
            _tokens = new ReadOnlyCollection<SymbolMarkupToken>(tokens);
        }

        public ReadOnlyCollection<SymbolMarkupToken> Tokens
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
            if (other.Tokens.Count != _tokens.Count)
                return false;

            for (var i = 0; i < _tokens.Count; i++)
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