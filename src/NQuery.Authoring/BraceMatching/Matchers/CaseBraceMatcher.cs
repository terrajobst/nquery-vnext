using System;

namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class CaseBraceMatcher : PairedTokenBraceMatcher
    {
        public CaseBraceMatcher()
            : base(SyntaxKind.CaseKeyword, SyntaxKind.EndKeyword)
        {
        }
    }
}