using System;
using System.ComponentModel.Composition;

namespace NQuery.Authoring.BraceMatching
{
    [Export(typeof(IBraceMatcher))]
    internal sealed class ParenthesisBraceMatcher : PairedTokenBraceMatcher
    {
        public ParenthesisBraceMatcher()
            : base(SyntaxKind.LeftParenthesisToken, SyntaxKind.RightParenthesisToken)
        {
        }
    }
}