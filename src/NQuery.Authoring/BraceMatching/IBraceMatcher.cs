using System;

namespace NQuery.Authoring.BraceMatching
{
    public interface IBraceMatcher
    {
        BraceMatchingResult MatchBraces(SyntaxToken token, int position);
    }
}