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

        public static BraceMatchingResult MatchBraces(this SyntaxTree syntaxTree, int position)
        {
            var braceMatchers = GetStandardBraceMatchers();
            return syntaxTree.MatchBraces(position, braceMatchers);
        }

        public static BraceMatchingResult MatchBraces(this SyntaxTree syntaxTree, int position, IEnumerable<IBraceMatcher> braceMatchers)
        {
            return (from m in braceMatchers
                    let r = m.MatchBraces(syntaxTree, position)
                    where r.IsValid
                    select r).DefaultIfEmpty(BraceMatchingResult.None).First();
        }
    }
}