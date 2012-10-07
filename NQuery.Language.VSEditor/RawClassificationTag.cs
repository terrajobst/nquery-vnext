using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    internal sealed class RawClassificationTag
    {
        private readonly TextSpan _textSpan;
        private readonly Symbol _symbol;

        public RawClassificationTag(TextSpan textSpan, Symbol symbol)
        {
            _textSpan = textSpan;
            _symbol = symbol;
        }

        public TextSpan TextSpan
        {
            get { return _textSpan; }
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }
    }
}