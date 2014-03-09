using System;

namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class IdentifierBraceMatcher : SingleTokenBraceMatcher
    {
        public IdentifierBraceMatcher()
            : base(SyntaxKind.IdentifierToken)
        {
        }

        public override bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right)
        {
            var hasBraces = token.IsQuotedIdentifier() || token.IsParenthesizedIdentifier();
            if (!hasBraces)
            {
                left = default(TextSpan);
                right = default(TextSpan);
                return false;
            }

            return base.TryFindBrace(token, position, out left, out right);
        }
    }
}