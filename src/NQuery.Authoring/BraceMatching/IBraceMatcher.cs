using System;

using NQuery.Text;

namespace NQuery.Authoring.BraceMatching
{
    public interface IBraceMatcher
    {
        bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right);
    }
}