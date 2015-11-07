using System;

namespace NQuery.Syntax
{
    public abstract class QuerySyntax : SyntaxNode
    {
        internal QuerySyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}