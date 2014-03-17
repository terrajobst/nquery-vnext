using System;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.BraceMatching
{
    internal abstract class PairedTokenBraceMatcher : IBraceMatcher
    {
        private readonly SyntaxKind _leftKind;
        private readonly SyntaxKind _rightKind;

        protected PairedTokenBraceMatcher(SyntaxKind leftKind, SyntaxKind rightKind)
        {
            _leftKind = leftKind;
            _rightKind = rightKind;
        }

        public BraceMatchingResult MatchBraces(SyntaxToken token, int position)
        {
            var isLeft = token.Kind == _leftKind &&
                         position == token.Span.Start;

            var isRight = token.Kind == _rightKind &&
                          position == token.Span.End;

            if (isLeft)
            {
                TextSpan left = token.Span;
                TextSpan right;
                if (FindMatchingBrace(position, 1, token.Parent, _rightKind, out right))
                    return new BraceMatchingResult(left, right);
            }
            else if (isRight)
            {
                TextSpan left;
                TextSpan right = token.Span;
                if (FindMatchingBrace(position, -1, token.Parent, _leftKind, out left))
                    return new BraceMatchingResult(left, right);
            }

            return BraceMatchingResult.None;
        }

        private static bool FindMatchingBrace(int position, int direction, SyntaxNode parent, SyntaxKind syntaxKind, out TextSpan right)
        {
            var tokens = from t in parent.ChildNodesAndTokens()
                         where t.Kind == syntaxKind && t.IsToken
                         select t.AsToken();

            var relevantTokens = direction < 0
                                     ? from t in tokens
                                       where t.Span.End <= position
                                       select t
                                     : from t in tokens
                                       where position < t.Span.Start
                                       select t;

            right = new TextSpan();
            var found = false;

            foreach (var token in relevantTokens)
            {
                if (!found)
                {
                    right = token.Span;
                    found = true;
                }
                else
                    return false;
            }

            return found;
        }
    }
}