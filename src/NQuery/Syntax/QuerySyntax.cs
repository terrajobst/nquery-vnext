namespace NQuery.Syntax
{
    public abstract class QuerySyntax : SyntaxNode
    {
        private protected QuerySyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}