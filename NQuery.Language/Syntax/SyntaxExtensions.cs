namespace NQuery.Language
{
    public static class SyntaxExtensions
    {
        public static SyntaxToken FindTokenOnLeft(this SyntaxNode root, int position)
        {
            var token = root.FindToken(position, descendIntoTrivia:true);
            return token.GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(position);
        }

        public static SyntaxToken GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(this SyntaxToken token, int position)
        {
            var previous = token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true);
            if (previous != null)
            {
                if (token.Kind == SyntaxKind.EndOfFileToken || previous.Span.End == position)
                    return previous;
            }

            return token;
        }

        public static SyntaxToken FindTokenContext(this SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);

            // In case the previous or next token is a missing token, we'll use this
            // one instead.

            if (!token.Span.ContainsOrTouches(position))
            {
                // token <missing> | token
                var previousToken = token.GetPreviousToken(includeZeroLength: true);
                if (previousToken != null && previousToken.IsMissing && previousToken.Span.End <= position)
                    return previousToken;

                // token | <missing> token
                var nextToken = token.GetNextToken(includeZeroLength: true);
                if (nextToken != null && nextToken.IsMissing && position <= nextToken.Span.Start)
                    return nextToken;
            }

            return token;
        }
    }
}