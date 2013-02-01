using System;
using System.Linq;

using NQuery.Syntax;

namespace NQuery
{
    public static class SyntaxExtensions
    {
        public static SyntaxToken FindTokenOnLeft(this SyntaxNode root, int position)
        {
            var token = root.FindToken(position, descendIntoTrivia:true);
            return token.GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(position);
        }

        public static SyntaxToken GetPreviousTokenIfEndOfFile(this SyntaxToken token)
        {
            return token.Kind != SyntaxKind.EndOfFileToken
                       ? token
                       : token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true) ?? token;
        }

        private static SyntaxToken GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(this SyntaxToken token, int position)
        {
            var previous = token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true);
            if (previous != null)
            {
                if (token.Kind == SyntaxKind.EndOfFileToken || previous.Span.End == position)
                    return previous;
            }

            return token;
        }

        public static SyntaxToken GetPreviousIfCurrentContainsOrTouchesPosition(this SyntaxToken token, int position)
        {
            return token != null && token.Span.ContainsOrTouches(position)
                       ? token.GetPreviousToken()
                       : token;
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

        public static bool InComment(this SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);
            return (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                    where t.Span.ContainsOrTouches(position)
                    where t.Kind == SyntaxKind.SingleLineCommentTrivia ||
                          t.Kind == SyntaxKind.MultiLineCommentTrivia
                    select t).Any();
        }

        public static bool InLiteral(this SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);
            return token.Span.ContainsOrTouches(position) && token.Kind.IsLiteral();
        }

        public static bool InUserGivenName(this SyntaxNode root, int position)
        {
            return root.InAlias(position) ||
                   root.InCteName(position) ||
                   root.InCteColumnList(position) ||
                   root.InDerivedTableName(position);
        }

        private static bool InAlias(this SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);
            var node = token.Parent as AliasSyntax;
            return node != null && node.Span.ContainsOrTouches(position);
        }

        private static bool InCteName(this SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);
            var cte = token.Parent as CommonTableExpressionSyntax;
            return cte != null && cte.Name.Span.ContainsOrTouches(position);
        }

        private static bool InCteColumnList(this SyntaxNode root, int position)
        {
            var node = root.FindTokenOnLeft(position).Parent;
            return node.Span.ContainsOrTouches(position) &&
                   (node is CommonTableExpressionColumnNameSyntax ||
                    node is CommonTableExpressionColumnNameListSyntax);
        }

        private static bool InDerivedTableName(this SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenOnLeft(position);
            var derivedTable = syntaxToken.Parent as DerivedTableReferenceSyntax;
            return derivedTable != null && derivedTable.Name.FullSpan.ContainsOrTouches(position);
        }

    }
}