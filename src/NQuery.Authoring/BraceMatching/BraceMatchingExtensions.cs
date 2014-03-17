using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Authoring.BraceMatching.Matchers;
using NQuery.Text;

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
            return (from t in syntaxTree.Root.FindStartTokens(position)
                    let r = FindBrace(t, position, braceMatchers)
                    where r.IsValid
                    select r).DefaultIfEmpty(BraceMatchingResult.None).First();
        }

        private static BraceMatchingResult FindBrace(SyntaxToken token, int position, IEnumerable<IBraceMatcher> braceMatchers)
        {
            foreach (var braceMatcher in braceMatchers)
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