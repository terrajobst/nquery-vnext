namespace NQuery.Syntax
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        internal StructuredTriviaSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }

        public SyntaxTrivia ParentTrivia
        {
            get { return SyntaxTree?.GetParentTrivia(this); }
        }
    }
}