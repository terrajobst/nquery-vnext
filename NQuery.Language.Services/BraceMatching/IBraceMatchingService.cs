using System;

namespace NQuery.Language.Services.BraceMatching
{
    public interface IBraceMatchingService
    {
        bool TryFindBrace(SyntaxTree syntaxTree, int position, out TextSpan left, out TextSpan right);
    }
}