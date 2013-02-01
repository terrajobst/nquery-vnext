using System;

namespace NQuery.Syntax
{
    public abstract class SelectColumnSyntax : SyntaxNode
    {
        protected SelectColumnSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}