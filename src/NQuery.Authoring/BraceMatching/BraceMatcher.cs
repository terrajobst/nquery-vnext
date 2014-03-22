using System;
using System.Linq;

namespace NQuery.Authoring.BraceMatching
{
    public abstract class BraceMatcher : IBraceMatcher
    {
        public BraceMatchingResult MatchBraces(SyntaxTree syntaxTree, int position)
        {
            return (from t in syntaxTree.Root.FindStartTokens(position)
                let r = MatchBraces(t, position)
                where r.IsValid
                select r).DefaultIfEmpty(BraceMatchingResult.None).First();
        }

        protected abstract BraceMatchingResult MatchBraces(SyntaxToken syntaxTree, int position);
    }
}