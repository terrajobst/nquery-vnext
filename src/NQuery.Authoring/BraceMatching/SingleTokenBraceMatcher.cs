using System;

using NQuery.Text;

namespace NQuery.Authoring.BraceMatching
{
    internal abstract class SingleTokenBraceMatcher : IBraceMatcher
    {
        private readonly SyntaxKind _tokenKind;

        protected SingleTokenBraceMatcher(SyntaxKind tokenKind)
        {
            _tokenKind = tokenKind;
        }

        public virtual BraceMatchingResult MatchBraces(SyntaxToken token, int position)
        {
            var startOrEnd = position == token.Span.Start ||
                             position == token.Span.End;

            if (token.Kind != _tokenKind || !startOrEnd || !token.IsTerminated())
                return BraceMatchingResult.None;

            var left = new TextSpan(token.Span.Start, 1);
            var right = new TextSpan(token.Span.End - 1, 1);
            return new BraceMatchingResult(left, right);
        }
    }
}