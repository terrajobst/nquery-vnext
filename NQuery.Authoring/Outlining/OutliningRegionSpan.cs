namespace NQuery.Language.Services.Outlining
{
    public struct OutliningRegionSpan
    {
        private readonly TextSpan _span;
        private readonly string _text;

        public OutliningRegionSpan(TextSpan span, string text)
        {
            _span = span;
            _text = text;
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public string Text
        {
            get { return _text; }
        }
    }
}