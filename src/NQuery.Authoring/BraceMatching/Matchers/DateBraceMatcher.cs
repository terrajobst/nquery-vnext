using System;

namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class DateBraceMatcher : SingleTokenBraceMatcher
    {
        public DateBraceMatcher()
            : base(SyntaxKind.DateLiteralToken)
        {
        }
    }
}