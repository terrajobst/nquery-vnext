namespace NQuery.Syntax
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        private protected StructuredTriviaSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }

        public SyntaxTrivia ParentTrivia
        {
            get { return SyntaxTree?.GetParentTrivia(this); }
        }
    }
}