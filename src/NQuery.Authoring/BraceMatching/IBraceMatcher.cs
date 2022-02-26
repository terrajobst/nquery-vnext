namespace NQuery.Authoring.BraceMatching
{
    public interface IBraceMatcher
    {
        BraceMatchingResult MatchBraces(SyntaxTree syntaxTree, int position);
    }
}