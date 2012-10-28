using System;

namespace NQuery.Authoring.BraceMatching
{
    public interface IBraceMatchingService
    {
        bool TryFindBrace(SyntaxTree syntaxTree, int position, out TextSpan left, out TextSpan right);
    }
}