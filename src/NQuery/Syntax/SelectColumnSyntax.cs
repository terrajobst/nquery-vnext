using System;

namespace NQuery
{
    public abstract class SelectColumnSyntax : SyntaxNode
    {
        protected SelectColumnSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}