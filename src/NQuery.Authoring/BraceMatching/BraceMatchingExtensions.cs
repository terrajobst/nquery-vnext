using System;
using System.Collections.Generic;

using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.BraceMatching
{
    public static class BraceMatchingExtensions
    {
        public static IEnumerable<IBraceMatcher> GetStandardBraceMatchers()
        {
            return new IBraceMatcher[]
                   {
                       new StringQuoteBraceMatcher(),
                       new CaseBraceMatcher(),
                       new DateBraceMatcher(),
                       new IdentifierBraceMatcher(),
                       new ParenthesisBraceMatcher(),
                   };
        }

        public static BraceMatchingResult FindBrace(this SyntaxTree syntaxTree, int position)
        {
            var braceMatchers = GetStandardBraceMatchers();
            return syntaxTree.FindBrace(position, braceMatchers);
        }

        public static BraceMatchingResult FindBrace(this SyntaxTree syntaxTree, int position, IEnumerable<IBraceMatcher> braceMatchers)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var result = FindBrace(token, position, braceMatchers);
            if (result.IsValid)
                return result;

            var previousToken = token.GetPreviousToken();
            if (previousToken != null && previousToken.Span.End == token.Span.Start)
            {
                result = FindBrace(previousToken, position, braceMatchers);
                if (result.IsValid)
                    return result;
            }

            var nextToken = token.GetPreviousToken();
            if (nextToken != null && nextToken.Span.Start == token.Span.End)
            {
                result = FindBrace(nextToken, position, braceMatchers);
                if (result.IsValid)
                    return result;
            }

            return BraceMatchingResult.None;
        }

        private static BraceMatchingResult FindBrace(SyntaxToken token, int position, IEnumerable<IBraceMatcher> getDefaultBraceMatchers)
        {
            foreach (var braceMatcher in getDefaultBraceMatchers)
            {
                TextSpan left;
                TextSpan right;
                if (braceMatcher.TryFindBrace(token, position, out left, out right))
                    return new BraceMatchingResult(left, right);
            }
            return BraceMatchingResult.None;
        }
    }
}