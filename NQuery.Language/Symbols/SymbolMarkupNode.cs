namespace NQuery.Language.Symbols
{
    public sealed class SymbolMarkupNode
    {
        private readonly SymbolMarkupKind _kind;
        private readonly string _text;

        public SymbolMarkupNode(SymbolMarkupKind kind, string text)
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