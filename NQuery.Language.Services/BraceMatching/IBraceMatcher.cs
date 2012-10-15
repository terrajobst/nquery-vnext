namespace NQuery.Language.VSEditor.BraceMatching
{
    public interface IBraceMatcher
    {
        bool TryFindBrace(SyntaxToken token, int position, out TextSpan left, out TextSpan right);
    }
}