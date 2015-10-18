using System;
using System.Linq;

namespace NQuery.Authoring.BraceMatching
{
    public abstract class BraceMatcher : IBraceMatcher
    {
        public BraceMatchingResult MatchBraces(SyntaxTree syntaxTree, int position)
        {
            return syntaxTree.Root.FindStartTokens(position)
                                  .Select(t => MatchBraces(t, position))
                                  .Where(r => r.IsValid)
                                  .DefaultIfEmpty(BraceMatchingResult.None)
                                  .First();
        }

        protected abstract BraceMatchingResult MatchBraces(SyntaxToken syntaxTree, int position);
    }
}