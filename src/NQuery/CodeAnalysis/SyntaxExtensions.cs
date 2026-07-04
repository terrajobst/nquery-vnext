using NQuery.CodeAnalysis.Syntax;

namespace NQuery.CodeAnalysis;

public static class SyntaxExtensions
{
    extension(SyntaxNode root)
    {
        public SyntaxToken FindTokenOnLeft(int position)
        {
            ThrowIfNull(root);

            var token = root.FindToken(position, descendIntoTrivia: true);
            return token.GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(position);
        }

        public IEnumerable<SyntaxToken> FindStartTokens(int position)
        {
            ThrowIfNull(root);

            var token = root.FindToken(position);
            yield return token;

            var previousToken = token.GetPreviousToken();
            if (previousToken is not null && previousToken.Span.End == position)
                yield return previousToken;
        }

        public IEnumerable<SyntaxNode> FindNodes(int position)
        {
            ThrowIfNull(root);

            // NOTE: We don't use Distinct() because we want to preserve the
            //       order of nodes.
            var seenNodes = new HashSet<SyntaxNode>();
            return root.FindStartTokens(position)
                       .SelectMany(t => t.Parent?.AncestorsAndSelf() ?? [])
                       .Where(seenNodes.Add);
        }

        public IEnumerable<T> FindNodes<T>(int position)
            where T : SyntaxNode
        {
            ThrowIfNull(root);

            return root.FindNodes(position).OfType<T>();
        }

        public SyntaxToken FindTokenContext(int position)
        {
            ThrowIfNull(root);

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

        public bool InComment(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                    where t.Span.ContainsOrTouches(position)
                    where t.Kind == SyntaxKind.SingleLineCommentTrivia ||
                          t.Kind == SyntaxKind.MultiLineCommentTrivia
                    select t).Any();
        }

        public bool InLiteral(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Span.ContainsOrTouches(position) && token.Kind.IsLiteral();
        }

        public bool GuaranteedInUserGivenName(int position)
        {
            ThrowIfNull(root);

            return root.GuaranteedInAlias(position) ||
                   root.GuaranteedInCteName(position) ||
                   root.InCteColumnList(position) ||
                   root.InDerivedTableName(position);
        }

        public bool PossiblyInUserGivenName(int position)
        {
            ThrowIfNull(root);

            return root.PossiblyInAlias(position) ||
                   root.PossiblyInCteName(position) ||
                   root.InCteColumnList(position) ||
                   root.InDerivedTableName(position);
        }

        private bool GuaranteedInAlias(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            var node = token.Parent as AliasSyntax;
            return node?.AsKeyword is not null && node.AsKeyword.Span.End <= position;
        }

        private bool PossiblyInAlias(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Parent is AliasSyntax node && node.Span.ContainsOrTouches(position);
        }

        private bool GuaranteedInCteName(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            var cte = token.Parent as CommonTableExpressionSyntax;
            return cte?.RecursiveKeyword is not null && cte.IdentifierToken.Span.ContainsOrTouches(position);
        }

        private bool PossiblyInCteName(int position)
        {
            ThrowIfNull(root);

            var token = root.FindTokenOnLeft(position);
            return token.Parent is CommonTableExpressionSyntax cte && cte.IdentifierToken.Span.ContainsOrTouches(position);
        }

        private bool InCteColumnList(int position)
        {
            ThrowIfNull(root);

            var node = root.FindTokenOnLeft(position).Parent;
            return node is not null &&
                   node.Span.ContainsOrTouches(position) &&
                   (node is CommonTableExpressionColumnNameSyntax ||
                    node is CommonTableExpressionColumnNameListSyntax);
        }

        private bool InDerivedTableName(int position)
        {
            ThrowIfNull(root);

            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Parent is DerivedTableReferenceSyntax derivedTable && derivedTable.IdentifierToken.FullSpan.ContainsOrTouches(position);
        }
    }

    extension(SyntaxToken token)
    {
        public SyntaxToken GetPreviousTokenIfEndOfFile()
        {
            ThrowIfNull(token);

            return token.Kind != SyntaxKind.EndOfFileToken
                       ? token
                       : token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true) ?? token;
        }

        private SyntaxToken GetPreviousTokenIfTouchingEndOrCurrentIsEndOfFile(int position)
        {
            var previous = token.GetPreviousToken(includeZeroLength: false, includeSkippedTokens: true);
            if (previous is not null)
            {
                if (token.Kind == SyntaxKind.EndOfFileToken || previous.Span.End == position)
                    return previous;
            }

            return token;
        }
    }

    extension(SyntaxToken? token)
    {
        public SyntaxToken? GetPreviousIfCurrentContainsOrTouchesPosition(int position)
        {
            return token is not null && token.Span.ContainsOrTouches(position)
                       ? token.GetPreviousToken()
                       : token;
        }
    }

    extension(OrderedQuerySyntax query)
    {
        public SelectQuerySyntax? GetAppliedSelectQuery()
        {
            ThrowIfNull(query);

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
