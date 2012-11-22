using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class SymbolMarkup
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