using System;
using System.ComponentModel.Composition;

namespace NQuery.Language.Services.BraceMatching
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