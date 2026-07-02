using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching.Matchers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.BraceMatching;

public static class BraceMatchingExtensions
{
    public static ImmutableArray<IBraceMatcher> StandardBraceMatchers { get; } =
    [
        new StringQuoteBraceMatcher(),
        new CaseBraceMatcher(),
        new DateBraceMatcher(),
        new IdentifierBraceMatcher(),
        new ParenthesisBraceMatcher(),
    ];

    extension(SyntaxTree syntaxTree)
    {
        public BraceMatchingResult MatchBraces(int position)
        {
            return syntaxTree.MatchBraces(position, StandardBraceMatchers);
        }

        public BraceMatchingResult MatchBraces(int position, IEnumerable<IBraceMatcher> braceMatchers)
        {
            return (from m in braceMatchers
                    let r = m.MatchBraces(syntaxTree, position)
                    where r.IsValid
                    select r).DefaultIfEmpty(BraceMatchingResult.None).First();
        }
    }
}
