using System;

namespace NQuery.Language
{
    public abstract class QuerySyntax : SyntaxNode
    {
        protected QuerySyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}