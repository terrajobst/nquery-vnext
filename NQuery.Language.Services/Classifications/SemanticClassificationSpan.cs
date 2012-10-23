namespace NQuery.Language.VSEditor
{
    public struct SemanticClassificationSpan
    {
        private readonly TextSpan _span;
        private readonly SemanticClassification _classification;

        public SemanticClassificationSpan(TextSpan span, SemanticClassification classification)
        {
            _span = span;
            _classification = classification;
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public SemanticClassification Classification
        {
            get { return _classification; }
        }
    }
}