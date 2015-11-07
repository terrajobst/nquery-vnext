using System;

using NQuery.Text;

namespace NQuery.Authoring.Classifications
{
    public struct SemanticClassificationSpan
    {
        public SemanticClassificationSpan(TextSpan span, SemanticClassification classification)
        {
            Span = span;
            Classification = classification;
        }

        public TextSpan Span { get; }

        public SemanticClassification Classification { get; }
    }
}