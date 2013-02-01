using System;

namespace NQuery.Syntax
{
    public abstract class QuerySyntax : SyntaxNode
    {
        protected QuerySyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}