using System;

namespace NQuery.Language.Services.BraceMatching
{
    public interface IBraceMatcher
    {
        bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right);
    }
}