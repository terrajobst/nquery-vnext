namespace NQuery.Authoring.BraceMatching.Matchers
{
    internal sealed class IdentifierBraceMatcher : SingleTokenBraceMatcher
    {
        public IdentifierBraceMatcher()
            : base(SyntaxKind.IdentifierToken)
        {
        }

        protected override BraceMatchingResult MatchBraces(SyntaxToken token, int position)
        {
            var hasBraces = token.IsQuotedIdentifier() || token.IsParenthesizedIdentifier();
            return hasBraces
                ? base.MatchBraces(token, position)
                : BraceMatchingResult.None;
        }
    }
}