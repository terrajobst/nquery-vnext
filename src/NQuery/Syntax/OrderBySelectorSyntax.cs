using System;

namespace NQuery.Syntax
{
    public abstract class OrderBySelectorSyntax : SyntaxNode
    {
        protected OrderBySelectorSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}