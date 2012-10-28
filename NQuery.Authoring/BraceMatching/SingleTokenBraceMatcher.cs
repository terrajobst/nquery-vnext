using System;

namespace NQuery.Authoring.BraceMatching
{
    internal abstract class SingleTokenBraceMatcher : IBraceMatcher
    {
        private readonly SyntaxKind _tokenKind;

        protected SingleTokenBraceMatcher(SyntaxKind tokenKind)
        {
            _tokenKind = tokenKind;
        }

        public virtual bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right)
        {
            var startOrEnd = position == token.Span.Start ||
                             position == token.Span.End;

            if (token.Kind != _tokenKind || !startOrEnd || !token.IsTerminated())
            {
                left = default(TextSpan);
                right = default(TextSpan);
                return false;
            }
            
            left = new TextSpan(token.Span.Start, 1);
            right = new TextSpan(token.Span.End - 1, 1);
            return true;
        }
    }
}