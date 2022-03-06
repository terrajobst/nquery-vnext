using NQuery.Syntax;

namespace NQuery
{
    public static class SyntaxExtensions
    {
        public static SyntaxToken FindTokenOnLeft(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindToken(position, descendIntoTrivia: true);
            return token.GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(position);
        }

        public static IEnumerable<SyntaxToken> FindStartTokens(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindToken(position);
            yield return token;

            var previousToken = token.GetPreviousToken();
            if (previousToken is not null && previousToken.Span.End == position)
                yield return previousToken;
        }

        public static IEnumerable<SyntaxNode> FindNodes(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            // NOTE: We don't use Distinct() because we want to preserve the
            //       order of nodes.
            var seenNodes = new HashSet<SyntaxNode>();
            return root.FindStartTokens(position)
                       .SelectMany(t => t.Parent.AncestorsAndSelf())
                       .Where(seenNodes.Add);
        }

        public static IEnumerable<T> FindNodes<T>(this SyntaxNode root, int position)
            where T : SyntaxNode
        {
            ArgumentNullException.ThrowIfNull(root);

            return root.FindNodes(position).OfType<T>();
        }

        public static SyntaxToken GetPreviousTokenIfEndOfFile(this SyntaxToken token)
        {
            ArgumentNullException.ThrowIfNull(token);

            return token.Kind != SyntaxKind.EndOfFileToken
                       ? token
                       : token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true) ?? token;
        }

        private static SyntaxToken GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(this SyntaxToken token, int position)
        {
            var previous = token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true);
            if (previous is not null)
            {
                if (token.Kind == SyntaxKind.EndOfFileToken || previous.Span.End == position)
                    return previous;
            }

            return token;
        }

        public static SyntaxToken GetPreviousIfCurrentContainsOrTouchesPosition(this SyntaxToken token, int position)
        {
            return token is not null && token.Span.ContainsOrTouches(position)
                       ? token.GetPreviousToken()
                       : token;
        }

        public static SyntaxToken FindTokenContext(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);

            // In case the previous or next token is a missing token, we'll use this
            // one instead.

            if (!token.Span.ContainsOrTouches(position))
            {
                // token <missing> | token
                var previousToken = token.GetPreviousToken(includeZeroLength: true);
                if (previousToken is not null && previousToken.IsMissing && previousToken.Span.End <= position)
                    return previousToken;

                // token | <missing> token
                var nextToken = token.GetNextToken(includeZeroLength: true);
                if (nextToken is not null && nextToken.IsMissing && position <= nextToken.Span.Start)
                    return nextToken;
            }

            return token;
        }

        public static bool InComment(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                    where t.Span.ContainsOrTouches(position)
                    where t.Kind == SyntaxKind.SingleLineCommentTrivia ||
                          t.Kind == SyntaxKind.MultiLineCommentTrivia
                    select t).Any();
        }

        public static bool InLiteral(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Span.ContainsOrTouches(position) && token.Kind.IsLiteral();
        }

        public static bool GuaranteedInUserGivenName(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            return root.GuaranteedInAlias(position) ||
                   root.GuaranteedInCteName(position) ||
                   root.InCteColumnList(position) ||
                   root.InDerivedTableName(position);
        }

        public static bool PossiblyInUserGivenName(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            return root.PossiblyInAlias(position) ||
                   root.PossiblyInCteName(position) ||
                   root.InCteColumnList(position) ||
                   root.InDerivedTableName(position);
        }

        private static bool GuaranteedInAlias(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            var node = token.Parent as AliasSyntax;
            return node?.AsKeyword is not null && node.AsKeyword.Span.End <= position;
        }

        private static bool PossiblyInAlias(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Parent is AliasSyntax node && node.Span.ContainsOrTouches(position);
        }

        private static bool GuaranteedInCteName(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            var cte = token.Parent as CommonTableExpressionSyntax;
            return cte?.RecursiveKeyword is not null && cte.Name.Span.ContainsOrTouches(position);
        }

        private static bool PossiblyInCteName(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Parent is CommonTableExpressionSyntax cte && cte.Name.Span.ContainsOrTouches(position);
        }

        private static bool InCteColumnList(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var node = root.FindTokenOnLeft(position).Parent;
            return node.Span.ContainsOrTouches(position) &&
                   (node is CommonTableExpressionColumnNameSyntax ||
                    node is CommonTableExpressionColumnNameListSyntax);
        }

        private static bool InDerivedTableName(this SyntaxNode root, int position)
        {
            ArgumentNullException.ThrowIfNull(root);

            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Parent is DerivedTableReferenceSyntax derivedTable && derivedTable.Name.FullSpan.ContainsOrTouches(position);
        }

        public static SelectQuerySyntax GetAppliedSelectQuery(this OrderedQuerySyntax query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var node = query.Query;

            while (node is ParenthesizedQuerySyntax)
            {
                var parenthesizedQuery = (ParenthesizedQuerySyntax)node;
                node = parenthesizedQuery.Query;
            }

            return node as SelectQuerySyntax;
        }
    }
}