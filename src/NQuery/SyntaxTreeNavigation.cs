using System.Collections.Immutable;
using NQuery.Syntax;

namespace NQuery
{
    internal static class SyntaxTreeNavigation
    {
        private static readonly Func<SyntaxToken, bool> AnyTokenPredicate = _ => true;
        private static readonly Func<SyntaxToken, bool> NonZeroLengthTokenPredicate = t => t.Span.Length > 0;

        private static readonly Func<SyntaxTrivia, bool> NoTriviaPredicate = _ => false;
        private static readonly Func<SyntaxTrivia, bool> SkippedTokensTriviaPredicate = t => t.Kind == SyntaxKind.SkippedTokensTrivia;

        private static Func<SyntaxToken, bool> GetTokenPredicate(bool includeZeroLength)
        {
            return includeZeroLength ? AnyTokenPredicate : NonZeroLengthTokenPredicate;
        }

        private static Func<SyntaxTrivia, bool> GetTriviaPredicate(bool includeSkippedTokens)
        {
            return includeSkippedTokens ? SkippedTokensTriviaPredicate : NoTriviaPredicate;
        }

        public static SyntaxToken GetFirstToken(SyntaxNode node, bool includeZeroLength, bool includeSkippedTokens)
        {
            ArgumentNullException.ThrowIfNull(node);

            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetFirstToken(node, tokenPredicate, triviaPredicate);
        }

        public static SyntaxToken GetLastToken(SyntaxNode node, bool includeZeroLength, bool includeSkippedTokens)
        {
            ArgumentNullException.ThrowIfNull(node);

            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetLastToken(node, tokenPredicate, triviaPredicate);
        }

        public static SyntaxToken GetPreviousToken(SyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            ArgumentNullException.ThrowIfNull(token);

            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetPreviousToken(token, true, tokenPredicate, triviaPredicate);
        }

        public static SyntaxToken GetNextToken(SyntaxToken token, bool includeZeroLength, bool includeSkippedTokens)
        {
            ArgumentNullException.ThrowIfNull(token);

            var tokenPredicate = GetTokenPredicate(includeZeroLength);
            var triviaPredicate = GetTriviaPredicate(includeSkippedTokens);
            return GetNextToken(token, true, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetFirstToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodesAndTokens())
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetFirstToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
                else
                {
                    var t = GetFirstToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetFirstToken(SyntaxToken token, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            var lt = GetFirstToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
            if (lt is not null)
                return lt;

            if (tokenPredicate(token))
                return token;

            var tt = GetFirstToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
            return tt;
        }

        private static SyntaxToken GetFirstToken(IEnumerable<SyntaxTrivia> triviaList, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList)
            {
                if (triviaPredicate(trivia) && trivia.Structure is not null)
                {
                    var t = GetFirstToken(trivia.Structure, tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetLastToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            foreach (var nodeOrToken in node.ChildNodesAndTokens().Reverse())
            {
                if (nodeOrToken.IsToken)
                {
                    var t = GetLastToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
                else
                {
                    var t = GetLastToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetLastToken(SyntaxToken token, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            var tt = GetLastToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
            if (tt is not null)
                return tt;

            if (tokenPredicate(token))
                return token;

            var lt = GetLastToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
            return lt;
        }

        private static SyntaxToken GetLastToken(IEnumerable<SyntaxTrivia> triviaList, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            foreach (var trivia in triviaList.Reverse())
            {
                if (triviaPredicate(trivia) && trivia.Structure is not null)
                {
                    var t = GetLastToken(trivia.Structure, tokenPredicate, triviaPredicate);
                    if (t is not null)
                        return t;
                }
            }

            return null;
        }

        private static SyntaxToken GetPreviousToken(SyntaxToken token, bool searchLeadingTrivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (searchLeadingTrivia)
            {
                var lt = GetLastToken(token.LeadingTrivia, tokenPredicate, triviaPredicate);
                if (lt is not null)
                    return lt;
            }

            if (token.Parent is null)
                return null;

            var returnNext = false;

            foreach (var nodeOrToken in token.Parent.ChildNodesAndTokens().Reverse())
            {
                if (returnNext)
                {
                    if (nodeOrToken.IsToken)
                    {
                        var t = GetLastToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                        if (t is not null)
                            return t;
                    }
                    else
                    {
                        var t = GetLastToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                        if (t is not null)
                            return t;
                    }

                }
                else if (nodeOrToken.IsToken && nodeOrToken.AsToken() == token)
                {
                    returnNext = true;
                }
            }

            return GetPreviousToken(token.Parent, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (node.Parent is not null)
            {
                var returnNext = false;

                foreach (var nodeOrToken in node.Parent.ChildNodesAndTokens().Reverse())
                {
                    if (returnNext)
                    {
                        if (nodeOrToken.IsToken)
                        {
                            var t = GetLastToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                            if (t is not null)
                                return t;
                        }
                        else
                        {
                            var t = GetLastToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                            if (t is not null)
                                return t;
                        }
                    }

                    if (nodeOrToken.IsNode && nodeOrToken.AsNode() == node)
                        returnNext = true;
                }

                return GetPreviousToken(node.Parent, tokenPredicate, triviaPredicate);
            }

            if (node is not StructuredTriviaSyntax structuredTrivia)
                return null;

            var trivia = structuredTrivia.ParentTrivia;
            return trivia is null
                       ? null
                       : GetPreviousToken(trivia, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousToken(SyntaxTrivia trivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (trivia.Parent is null)
                return null;

            var tt = GetPreviousToken(trivia.Parent.TrailingTrivia, trivia, tokenPredicate, triviaPredicate);
            if (tt is not null)
                return tt;

            var lt = GetPreviousToken(trivia.Parent.LeadingTrivia, trivia, tokenPredicate, triviaPredicate);
            if (lt is not null)
                return lt;

            return GetPreviousToken(trivia.Parent, false, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetPreviousToken(ImmutableArray<SyntaxTrivia> triviaList, SyntaxTrivia trivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            var returnNext = false;

            foreach (var otherTrivia in triviaList.Reverse())
            {
                if (returnNext)
                {
                    var structure = otherTrivia.Structure;
                    if (triviaPredicate(otherTrivia) && structure is not null)
                    {
                        var token = GetLastToken(structure, tokenPredicate, triviaPredicate);
                        if (token is not null)
                            return token;
                    }
                }
                else if (otherTrivia == trivia)
                {
                    returnNext = true;
                }
            }

            var isTrailing = triviaList == trivia.Parent.TrailingTrivia;
            if (returnNext && isTrailing && tokenPredicate(trivia.Parent))
                return trivia.Parent;

            return null;
        }

        private static SyntaxToken GetNextToken(SyntaxToken token, bool searchTrailingTrivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (searchTrailingTrivia)
            {
                var tt = GetFirstToken(token.TrailingTrivia, tokenPredicate, triviaPredicate);
                if (tt is not null)
                    return tt;
            }

            if (token.Parent is null)
                return null;

            var returnNext = false;

            foreach (var nodeOrToken in token.Parent.ChildNodesAndTokens())
            {
                if (returnNext)
                {
                    if (nodeOrToken.IsToken)
                    {
                        var t = GetFirstToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                        if (t is not null)
                            return t;
                    }
                    else
                    {
                        var t = GetFirstToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                        if (t is not null)
                            return t;
                    }

                }
                else if (nodeOrToken.IsToken && nodeOrToken.AsToken() == token)
                {
                    returnNext = true;
                }
            }

            return GetNextToken(token.Parent, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetNextToken(SyntaxNode node, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (node.Parent is not null)
            {
                var returnNext = false;

                foreach (var nodeOrToken in node.Parent.ChildNodesAndTokens())
                {
                    if (returnNext)
                    {
                        if (nodeOrToken.IsToken)
                        {
                            var t = GetFirstToken(nodeOrToken.AsToken(), tokenPredicate, triviaPredicate);
                            if (t is not null)
                                return t;
                        }
                        else
                        {
                            var t = GetFirstToken(nodeOrToken.AsNode(), tokenPredicate, triviaPredicate);
                            if (t is not null)
                                return t;
                        }
                    }

                    if (nodeOrToken.IsNode && nodeOrToken.AsNode() == node)
                        returnNext = true;
                }

                return GetNextToken(node.Parent, tokenPredicate, triviaPredicate);
            }

            if (node is not StructuredTriviaSyntax structuredTrivia)
                return null;

            var trivia = structuredTrivia.ParentTrivia;
            return trivia is null
                       ? null
                       : GetNextToken(trivia, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetNextToken(SyntaxTrivia trivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            if (trivia.Parent is null)
                return null;

            var lt = GetNextToken(trivia.Parent.LeadingTrivia, trivia, tokenPredicate, triviaPredicate);
            if (lt is not null)
                return lt;

            var tt = GetNextToken(trivia.Parent.TrailingTrivia, trivia, tokenPredicate, triviaPredicate);
            if (tt is not null)
                return tt;

            return GetNextToken(trivia.Parent, false, tokenPredicate, triviaPredicate);
        }

        private static SyntaxToken GetNextToken(ImmutableArray<SyntaxTrivia> triviaList, SyntaxTrivia trivia, Func<SyntaxToken, bool> tokenPredicate, Func<SyntaxTrivia, bool> triviaPredicate)
        {
            var returnNext = false;

            foreach (var otherTrivia in triviaList)
            {
                if (returnNext)
                {
                    var structure = otherTrivia.Structure;
                    if (triviaPredicate(otherTrivia) && structure is not null)
                    {
                        var token = GetFirstToken(structure, tokenPredicate, triviaPredicate);
                        if (token is not null)
                            return token;
                    }
                }
                else if (otherTrivia == trivia)
                {
                    returnNext = true;
                }
            }

            var isLeading = triviaList == trivia.Parent.LeadingTrivia;
            if (returnNext && isLeading && tokenPredicate(trivia.Parent))
                return trivia.Parent;

            return null;
        }
    }
}