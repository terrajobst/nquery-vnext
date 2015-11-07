using System;

using NQuery.Text;

namespace NQuery.Authoring.Classifications
{
    public struct SyntaxClassificationSpan
    {
        public SyntaxClassificationSpan(TextSpan span, SyntaxClassification classification)
        {
            Span = span;
            Classification = classification;
        }

        public TextSpan Span { get; }

        public SyntaxClassification Classification { get; }
    }
}