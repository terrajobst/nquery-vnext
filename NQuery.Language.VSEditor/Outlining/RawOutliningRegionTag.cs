namespace NQuery.Language.VSEditor
{
    internal sealed class RawOutliningRegionTag
    {
        private readonly TextSpan _textSpan;
        private readonly string _text;
        private readonly string _hint;

        public RawOutliningRegionTag(TextSpan textSpan, string text, string hint)
        {
            _textSpan = textSpan;
            _text = text;
            _hint = hint;
        }

        public TextSpan TextSpan
        {
            get { return _textSpan; }
        }

        public string Text
        {
            get { return _text; }
        }

        public string Hint
        {
            get { return _hint; }
        }
    }
}