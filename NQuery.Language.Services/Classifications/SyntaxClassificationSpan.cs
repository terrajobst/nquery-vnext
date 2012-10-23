namespace NQuery.Language.VSEditor
{
    public struct SyntaxClassificationSpan
    {
        private readonly TextSpan _span;
        private readonly SyntaxClassification _classification;

        public SyntaxClassificationSpan(TextSpan span, SyntaxClassification classification)
        {
            _span = span;
            _classification = classification;
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public SyntaxClassification Classification
        {
            get { return _classification; }
        }
    }
}