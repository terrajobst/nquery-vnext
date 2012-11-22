using System;

namespace NQuery.Symbols
{
    public sealed class SymbolMarkupToken
    {
        private readonly SymbolMarkupKind _kind;
        private readonly string _text;

        public SymbolMarkupToken(SymbolMarkupKind kind, string text)
        {
            _kind = kind;
            _text = text;
        }

        public SymbolMarkupKind Kind
        {
            get { return _kind; }
        }

        public string Text
        {
            get { return _text; }
        }
    }
}