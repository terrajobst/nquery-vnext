using System;

namespace NQuery
{
    public abstract class QuerySyntax : SyntaxNode
    {
        protected QuerySyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}