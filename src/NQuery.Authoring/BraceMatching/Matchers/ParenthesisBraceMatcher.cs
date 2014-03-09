using System;

namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class ParenthesisBraceMatcher : PairedTokenBraceMatcher
    {
        public ParenthesisBraceMatcher()
            : base(SyntaxKind.LeftParenthesisToken, SyntaxKind.RightParenthesisToken)
        {
        }
    }
}