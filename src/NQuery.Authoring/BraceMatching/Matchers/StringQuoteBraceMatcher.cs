using System;

namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class StringQuoteBraceMatcher : SingleTokenBraceMatcher
    {
        public StringQuoteBraceMatcher()
            : base(SyntaxKind.StringLiteralToken)
        {
        }
    }
}