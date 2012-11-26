using System;
using System.Linq;

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

        public bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right)
        {
            var isLeft = token.Kind == _leftKind &&
                         position == token.Span.Start;

            var isRight = token.Kind == _rightKind &&
                          position == token.Span.End;

            if (!isLeft && !isRight)
            {
                left = default(TextSpan);
                right = default(TextSpan);
                return false;
            }

            if (isLeft)
            {
                left = token.Span;
                return FindMatchingBrace(position, 1, token.Parent, _rightKind, out right);
            }

            right = token.Span;
            return FindMatchingBrace(position, -1, token.Parent, _leftKind, out left);
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