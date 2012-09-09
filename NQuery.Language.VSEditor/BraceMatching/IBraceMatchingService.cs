namespace NQuery.Language.VSEditor.BraceMatching
{
    public interface IBraceMatchingService
    {
        bool TryFindBrace(SyntaxTree syntaxTree, int position, out TextSpan left, out TextSpan right);
    }
}